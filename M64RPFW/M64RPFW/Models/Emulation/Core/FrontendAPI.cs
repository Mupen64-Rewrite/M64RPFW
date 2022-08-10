using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using M64RPFW.Models.Helpers;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Models.Emulation.Core;

public static partial class Mupen64Plus
{
    // Public API
    // ========================================================
#pragma warning disable CS8618
    /// <summary>
    /// Initializes the Mupen64Plus bindings, loading it from the specified path.
    /// </summary>
    /// <param name="path">The path to <c>mupen64plus.dll</c> or equivalent</param>
    public static unsafe void Startup(string path)
    {
        if (Initialized)
            throw new InvalidOperationException("Mupen64Plus.Startup() was already called");
        _libHandle = NativeLibrary.Load(path);
        Initialized = true;

        ResolveFrontendFunctions();
        ResolveConfigFunctions();

        Error err = _fnCoreStartup(
            0x020000, null, null, (IntPtr) (int) PluginType.Core,
            OnDebug, IntPtr.Zero, OnStateChange);
        ThrowForError(err);


        _frameCallback = OnFrameComplete;
        err = _fnCoreDoCommand(Command.SetFrameCallback, 0,
            Marshal.GetFunctionPointerForDelegate(_frameCallback).ToPointer());
        ThrowForError(err);

        _pluginDict = new();
    }

    private static FrameCallback _frameCallback;

    /// <summary>
    /// Initializes the Mupen64Plus bindings, loading from
    /// <c>(path to exe dir)\Libs\mupen64plus.dll</c> or equivalent.
    /// </summary>
    public static void Startup()
    {
        Startup(GetExpectedLibPath());
    }

    public static void Shutdown()
    {
        Initialized = false;

        Error err = _fnCoreShutdown();
        ThrowForError(err);
    }

    public static bool Initialized { get; private set; }

    private static void ThrowIfNotInited(string name)
    {
        if (!Initialized)
            throw new InvalidOperationException($"Cannot access {name}() before calling Mupen64Plus.Startup()");
    }

    private static void OnDebug(IntPtr context, MessageLevel level, string message)
    {
        var type = (PluginType) (int) context;

        string typeString = type switch
        {
            PluginType.Core => "",
            PluginType.Graphics => "VIDEO ",
            PluginType.Audio => "AUDIO ",
            PluginType.Input => "INPUT ",
            PluginType.RSP => "RSP   ",
        };

        string levelString = level switch
        {
            MessageLevel.Error => "ERROR",
            MessageLevel.Warning => "WARN ",
            MessageLevel.Info => "INFO ",
            MessageLevel.Status => "STAT ",
            MessageLevel.Verbose => "TRACE"
        };

        Console.WriteLine($"[M64+ {typeString}{levelString}] {message}");
    }

    private static void OnStateChange(IntPtr context, CoreParam param, int newValue)
    {
        StateChanged?.Invoke(null, new StateChangeEventArgs { Param = param, NewValue = newValue });
    }

    private static void OnFrameComplete(int frameIndex)
    {
        FrameComplete?.Invoke(null, frameIndex);
    }

#pragma warning restore CS8618

    public class StateChangeEventArgs : EventArgs
    {
        public CoreParam Param { get; init; }
        public int NewValue { get; init; }
    }

    public static EventHandler<StateChangeEventArgs>? StateChanged;
    public static EventHandler<int>? FrameComplete;

    #region Core Commands

    public static unsafe void OpenRom(string path)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        ArgumentNullException.ThrowIfNull(path);
        
        byte[] bytes = File.ReadAllBytes(path);
        RomHelper.AdaptiveByteSwap(ref bytes);
        
        fixed (byte* bytesPtr = bytes)
        {
            Error err = _fnCoreDoCommand(Command.RomOpen, bytes.Length, bytesPtr);
            ThrowForError(err);
        }
    }
    
    /// <summary>
    /// Opens a ROM that is already loaded into memory. Assumes that said ROM is in the
    /// correct byte order.
    /// </summary>
    /// <param name="romData">The ROM to load</param>
    public static unsafe void OpenRomBinary(byte[] romData)
    {
        fixed (byte* dataPtr = romData)
        {
            Error err = _fnCoreDoCommand(Command.RomOpen, romData.Length, dataPtr);
            ThrowForError(err);
        }
    }

    public static unsafe void CloseRom()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnCoreDoCommand(Command.RomClose, 0, null);
        ThrowForError(err);
    }

    public static unsafe RomHeader GetRomHeader()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        int size = Marshal.SizeOf<RomHeader>();
        IntPtr alloc = Marshal.AllocHGlobal(size);
        try
        {
            Error err = _fnCoreDoCommand(Command.RomGetHeader, size, alloc.ToPointer());
            ThrowForError(err);
            var res = Marshal.PtrToStructure<RomHeader>(alloc);
            return res;
        }
        finally
        {
            Marshal.FreeHGlobal(alloc);
        }
    }

    public static unsafe RomSettings GetRomSettings()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        int size = Marshal.SizeOf<RomSettings>();
        IntPtr alloc = Marshal.AllocHGlobal(size);
        try
        {
            Error err = _fnCoreDoCommand(Command.RomGetSettings, size, alloc.ToPointer());
            ThrowForError(err);
            var res = Marshal.PtrToStructure<RomSettings>(alloc);
            return res;
        }
        finally
        {
            Marshal.FreeHGlobal(alloc);
        }
    }

    public static unsafe void Execute()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        Error err = _fnCoreDoCommand(Command.Execute, 0, null);
        ThrowForError(err);
    }

    public static unsafe void Stop()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        Error err = _fnCoreDoCommand(Command.Stop, 0, null);
        ThrowForError(err);
    }

    public static unsafe void Pause()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        Error err = _fnCoreDoCommand(Command.Pause, 0, null);
        ThrowForError(err);
    }

    public static unsafe void Resume()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        Error err = _fnCoreDoCommand(Command.Resume, 0, null);
        ThrowForError(err);
    }

    public static unsafe int CoreStateQuery(CoreParam param)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        int res = 0;
        Error err = _fnCoreDoCommand(Command.CoreStateQuery, (int) param, &res);
        ThrowForError(err);

        return res;
    }

    public static unsafe void CoreStateSet(CoreParam param, int value)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        Error err = _fnCoreDoCommand(Command.CoreStateSet, (int) param, &value);
        ThrowForError(err);
    }

    public static unsafe void LoadStateFromFile(string path)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        ArgumentNullException.ThrowIfNull(path);

        IntPtr alloc = Marshal.StringToHGlobalAnsi(path);
        try
        {
            Error err = _fnCoreDoCommand(Command.StateLoad, 0, alloc.ToPointer());
            ThrowForError(err);
        }
        finally
        {
            Marshal.FreeHGlobal(alloc);
        }
    }

    public static unsafe void LoadStateFromCurrentSlot()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        Error err = _fnCoreDoCommand(Command.StateLoad, 0, null);
        ThrowForError(err);
    }

    public static unsafe void SaveStateToFile(string path, SavestateType type = SavestateType.Mupen64Plus)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        ArgumentNullException.ThrowIfNull(path);

        string fullPath = Path.GetFullPath(path);
        IntPtr alloc = Marshal.StringToHGlobalAnsi(fullPath);
        try
        {
            Error err = _fnCoreDoCommand(Command.StateSave, (int) type, alloc.ToPointer());
            ThrowForError(err);
        }
        finally
        {
            Marshal.FreeHGlobal(alloc);
        }
    }

    public static unsafe void SaveStateToCurrentSlot()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        Error err = _fnCoreDoCommand(Command.StateSave, 0, null);
        ThrowForError(err);
    }

    public static unsafe void SetSavestateSlot(int slot)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        if (slot < 0 || slot > 9)
            throw new ArgumentOutOfRangeException(nameof(slot), "Savestate slots range from 0-9 (inclusive)");
        Error err = _fnCoreDoCommand(Command.StateSetSlot, slot, null);
        ThrowForError(err);
    }

    public static unsafe void Reset(bool hard = true)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        Error err = _fnCoreDoCommand(Command.Reset, hard ? 1 : 0, null);
        ThrowForError(err);
    }

    public static unsafe void AdvanceFrame()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        Error err = _fnCoreDoCommand(Command.AdvanceFrame, 0, null);
        ThrowForError(err);
    }

    #endregion

    #region Miscellaneous core functions

    public static void OverrideVideoExtension(IVideoExtension obj)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        ArgumentNullException.ThrowIfNull(obj);
        _vidextDelegates = new(obj);

        Error err = _fnCoreOverrideVidExt(_vidextDelegates.AsNative());
        ThrowForError(err);
    }

    public static void RemoveVideoExtension()
    {
        _vidextDelegates = null;
        _fnCoreOverrideVidExt(VideoExtensionFunctions.Empty);
    }

    public static unsafe void AttachPlugin(string path)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        IntPtr pluginLib = NativeLibrary.Load(path);
        // Implicitly
        var getVersion = NativeLibHelper.GetFunction<DPluginGetVersion>(pluginLib, "PluginGetVersion");
        
        Error err = getVersion(out var type, out _, out _, out _, out _);

        if (!_pluginDict.TryAdd(type, pluginLib))
        {
            IntPtr oldLib = _pluginDict[type];
            var oldLibGetVersion = NativeLibHelper.GetFunction<DPluginGetVersion>(pluginLib, "PluginGetVersion");

            err = oldLibGetVersion(out _, out _, out _, out var oldNameBytes, out _);
            ThrowForError(err);
            
            // Manually strlen() oldNameBytes, then convert to string
            int oldNameLen = 0;
            while (oldNameBytes[oldNameLen] != 0) oldNameLen++;
            string oldName = Encoding.ASCII.GetString(oldNameBytes, oldNameLen);

            NativeLibrary.Free(pluginLib);
            throw new InvalidOperationException(
                $"Plugin type {type} already has a plugin registered ({oldName})");
        }
        

        var startup = NativeLibHelper.GetFunction<DPluginStartup>(pluginLib, "PluginStartup");
        err = startup(_libHandle, (IntPtr) (int) type, OnDebug);
        ThrowForError(err);

        err = _fnCoreAttachPlugin(type, pluginLib);
        ThrowForError(err);
    }

    public static void DetachPlugin(PluginType type)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        _pluginDict.Remove(type, out IntPtr pluginLib);

        Error err = _fnCoreDetachPlugin(type);
        ThrowForError(err);

        var shutdown = NativeLibHelper.GetFunction<DPluginShutdown>(pluginLib, "PluginShutdown");
        err = shutdown();
        ThrowForError(err);

        NativeLibrary.Free(pluginLib);
    }

    #endregion


    // Utilities
    // =================================
    private static string GetExpectedLibPath()
    {
        var asLib = ((Func<Func<string, string>>) (() =>
        {
            if (OperatingSystem.IsWindows())
                return s => $"{s}.dll";
            if (OperatingSystem.IsLinux())
                return s => $"{s}.so";
            throw new NotSupportedException("Your OS is not supported");
        }))();

        string path = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) ??
                      throw new ApplicationException("Could not retrieve .exe path");

        return Path.Join(new[] { path, "Libraries", asLib("mupen64plus") });
    }

    private static IntPtr _libHandle;
}