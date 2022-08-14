using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using M64RPFW.Models.Helpers;

namespace M64RPFW.Models.Emulation.Core;

public static partial class Mupen64Plus
{
    // Public API
    // ========================================================
#pragma warning disable CS8618
    static unsafe Mupen64Plus()
    {
        _libHandle = NativeLibrary.Load(GetExpectedLibPath());

        ResolveFrontendFunctions();
        ResolveConfigFunctions();

        _debugCallback = OnDebug;
        _stateCallback = OnStateChange;

        Error err = _fnCoreStartup(
            0x020000, null, null, (IntPtr) (int) PluginType.Core,
            _debugCallback, IntPtr.Zero, _stateCallback);
        ThrowForError(err);


        _frameCallback = OnFrameComplete;
        err = _fnCoreDoCommand(Command.SetFrameCallback, 0,
            Marshal.GetFunctionPointerForDelegate(_frameCallback).ToPointer());
        ThrowForError(err);

        _pluginDict = new();

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            // ReSharper disable once VariableHidesOuterVariable
            Error err = _fnCoreShutdown!();
            ThrowForError(err);
            
            NativeLibrary.Free(_libHandle);
        };
    }

    private static void ThrowIfNotInited([CallerMemberName] string name = "")
    {
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
            _ => "??    "
        };

        string levelString = level switch
        {
            MessageLevel.Error => "ERROR",
            MessageLevel.Warning => "WARN ",
            MessageLevel.Info => "INFO ",
            MessageLevel.Status => "STAT ",
            MessageLevel.Verbose => "TRACE",
            _ => "??   "
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

    public static event EventHandler<StateChangeEventArgs>? StateChanged;
    public static event EventHandler<int>? FrameComplete;

    #region Core Commands

    public static unsafe void OpenRom(string path)
    {
        
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

    /// <summary>
    /// Close the currently open ROM.
    /// </summary>
    public static unsafe void CloseRom()
    {
        

        Error err = _fnCoreDoCommand(Command.RomClose, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Get the ROM header of the currently open ROM.
    /// </summary>
    /// <returns>the ROM header of the currently open ROM</returns>
    public static unsafe RomHeader GetRomHeader()
    {
        

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

    /// <summary>
    /// Get the ROM "settings" (internal M64+ data) for the currently
    /// open ROM.
    /// </summary>
    /// <returns>The ROM settings for the currently open ROM</returns>
    public static unsafe RomSettings GetRomSettings()
    {
        

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

    /// <summary>
    /// Run the current ROM in this thread. This function blocks until the emulator is stopped.
    /// </summary>
    public static unsafe void Execute()
    {
        
        Error err = _fnCoreDoCommand(Command.Execute, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Stops the emulator. The emulator must not be stopped.
    /// </summary>
    public static unsafe void Stop()
    {
        
        Error err = _fnCoreDoCommand(Command.Stop, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Pauses the emulator. The emulator must be running.
    /// </summary>
    public static unsafe void Pause()
    {
        
        Error err = _fnCoreDoCommand(Command.Pause, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Resumes the emulator. The emulator must be paused.
    /// </summary>
    public static unsafe void Resume()
    {
        
        Error err = _fnCoreDoCommand(Command.Resume, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Gets one of the emulator parameters.
    /// </summary>
    /// <param name="param">the parameter to query</param>
    /// <returns>the parameter's value</returns>
    public static unsafe int CoreStateQuery(CoreParam param)
    {
        
        int res = 0;
        Error err = _fnCoreDoCommand(Command.CoreStateQuery, (int) param, &res);
        ThrowForError(err);

        return res;
    }

    /// <summary>
    /// Sets one of the emulator parameters.
    /// </summary>
    /// <param name="param">the parameter to set</param>
    /// <param name="value">the value to set to the parameter</param>
    public static unsafe void CoreStateSet(CoreParam param, int value)
    {
        
        Error err = _fnCoreDoCommand(Command.CoreStateSet, (int) param, &value);
        ThrowForError(err);
    }

    /// <summary>
    /// Loads the emulator's state from a file.
    /// </summary>
    /// <param name="path">path to the file</param>
    public static unsafe void LoadStateFromFile(string path)
    {
        
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

    /// <summary>
    /// Loads the emulator's state in the current slot.
    /// </summary>
    public static unsafe void LoadStateFromCurrentSlot()
    {
        
        Error err = _fnCoreDoCommand(Command.StateLoad, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Saves the emulator's state as a file.
    /// </summary>
    /// <param name="path">The path to save the savestate</param>
    /// <param name="type"></param>
    public static unsafe void SaveStateToFile(string path, SavestateType type = SavestateType.Mupen64Plus)
    {
        
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

    /// <summary>
    /// Saves the emulator's state to the current slot.
    /// </summary>
    public static unsafe void SaveStateToCurrentSlot()
    {
        
        Error err = _fnCoreDoCommand(Command.StateSave, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Sets the current savestate slot of the emulator.
    /// The slot indices range between 0-9 (inclusive).
    /// </summary>
    /// <param name="slot">the savestate slot to use</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="slot"/> is not between 0 and 9</exception>
    public static unsafe void SetSavestateSlot(int slot)
    {
        
        if (slot < 0 || slot > 9)
            throw new ArgumentOutOfRangeException(nameof(slot), "Savestate slots range from 0-9 (inclusive)");
        Error err = _fnCoreDoCommand(Command.StateSetSlot, slot, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Resets the emulator.
    /// </summary>
    /// <param name="hard">If true, performs a hard reset. Otherwise, performs a soft reset.</param>
    public static unsafe void Reset(bool hard = true)
    {
        
        Error err = _fnCoreDoCommand(Command.Reset, hard ? 1 : 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Advances the emulator by a single frame.
    /// </summary>
    public static unsafe void AdvanceFrame()
    {
        
        Error err = _fnCoreDoCommand(Command.AdvanceFrame, 0, null);
        ThrowForError(err);
    }

    #endregion

    #region Miscellaneous core functions

    /// <summary>
    /// Overrides the core's video extension API.
    /// </summary>
    /// <param name="obj">An object implementing the Video Extension API.</param>
    public static void OverrideVideoExtension(IVideoExtension obj)
    {
        
        ArgumentNullException.ThrowIfNull(obj);

        _vidextFunctions = new VideoExtensionFunctions(obj);

        Error err = _fnCoreOverrideVidExt(_vidextFunctions);
        ThrowForError(err);
    }

    /// <summary>
    /// Clears any active override on the core's video extension API.
    /// </summary>
    public static void RemoveVideoExtension()
    {
        _vidextFunctions = null;
        _fnCoreOverrideVidExt(VideoExtensionFunctions.Empty);
    }

    /// <summary>
    /// Attaches a plugin to the core.
    /// Mupen64Plus demands that they are attached in the following order:
    /// <list type="number">
    ///     <item>Graphics</item>
    ///     <item>Audio</item>
    ///     <item>Input</item>
    ///     <item>RSP</item>
    /// </list>
    /// </summary>
    /// <param name="path">Path to the plugin's .so file</param>
    /// <exception cref="InvalidOperationException">If the located plugin's type already has an attached plugin</exception>
    public static unsafe void AttachPlugin(string path)
    {
        
        IntPtr pluginLib = NativeLibrary.Load(path);
        // Implicitly
        var getVersion = NativeLibHelper.GetFunction<DPluginGetVersion>(pluginLib, "PluginGetVersion");

        Error err = getVersion(out var type, out _, out _, out _, out _);
        ThrowForError(err);

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

    /// <summary>
    /// Detaches a plugin from the core. Unlike <see cref="AttachPlugin"/>, does not mandate
    /// any sort of order.
    /// </summary>
    /// <param name="type">The plugin type to detach</param>
    public static void DetachPlugin(PluginType type)
    {
        
        _pluginDict.Remove(type, out var pluginLib);

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

        //return "/usr/lib/libmupen64plus.so";
    }

    private static IntPtr _libHandle;
}