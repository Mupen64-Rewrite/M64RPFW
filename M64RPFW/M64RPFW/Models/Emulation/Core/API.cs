using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Models.Emulation.Core;


public partial class Mupen64Plus
{
    // Public API
    // ========================================================
#pragma warning disable CS8618
    /// <summary>
    /// Initializes the Mupen64Plus bindings, loading it from the specified path.
    /// </summary>
    /// <param name="path">The path to <c>mupen64plus.dll</c> or equivalent</param>
    public Mupen64Plus(string path)
    {
        _libHandle = NativeLibrary.Load(path);

        // Resolve all needed symbols in advance. Warnings needed
        // to be disabled because that initialization takes place
        // in a function (outside the constructor).
        ResolveFrontendFunctions();

        Error err = _fnCoreStartup!(
            0x020000, null, null, (IntPtr)(int) PluginType.Core,
            OnDebug, IntPtr.Zero, OnStateChange);
        ThrowForError(err);

        err = CoreDoCommand(Command.SetFrameCallback, 0, new FrameCallback(OnFrameComplete));
        ThrowForError(err);
    }
    
    /// <summary>
    /// Initializes the Mupen64Plus bindings, loading from
    /// <c>(path to exe dir)\Libs\mupen64plus.dll</c> or equivalent.
    /// </summary>
    public Mupen64Plus() : this(GetExpectedLibPath()) {}

    private void OnDebug(IntPtr context, MessageLevel level, string message)
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

    private void OnStateChange(IntPtr context, CoreParam param, int newValue)
    {
        StateChanged(this, new StateChangeEventArgs { Param = param, NewValue = newValue });
    }

    private void OnFrameComplete(int frameIndex)
    {
        FrameComplete(this, frameIndex);
    }
    
#pragma warning restore CS8618

    public class StateChangeEventArgs : EventArgs
    {
        public CoreParam Param { get; init; }
        public int NewValue { get; init; }
    }

    public EventHandler<StateChangeEventArgs> StateChanged;

    public EventHandler<int> FrameComplete;

    ~Mupen64Plus()
    {
        ThrowForError(_fnCoreShutdown());
        NativeLibrary.Free(_libHandle);
    }

    #region Core Commands

    public void OpenROM(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        ROMHelper.AdaptiveByteSwap(ref bytes);

        Error err = CoreDoCommand(Command.RomOpen, 0, bytes);
        ThrowForError(err);
    }

    public void CloseROM()
    {
        Error err = CoreDoCommand(Command.RomClose, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public RomHeader GetROMHeader()
    {
        RomHeader header = new RomHeader();
        Error err = CoreDoCommand(Command.RomGetHeader, Marshal.SizeOf(header), ref header);
        ThrowForError(err);

        return header;
    }

    public RomHeader GetROMHeader(ref RomHeader header)
    {
        Error err = CoreDoCommand(Command.RomGetHeader, Marshal.SizeOf(header), ref header);
        ThrowForError(err);

        return header;
    }

    public RomSettings GetROMSettings()
    {
        RomSettings settings = new RomSettings();
        Error err = CoreDoCommand(Command.RomGetSettings, Marshal.SizeOf(settings), ref settings);
        ThrowForError(err);
        return settings;
    }

    public RomSettings GetROMSettings(ref RomSettings settings)
    {
        Error err = CoreDoCommand(Command.RomGetSettings, Marshal.SizeOf(settings), ref settings);
        ThrowForError(err);
        return settings;
    }

    public void Execute()
    {
        Error err = CoreDoCommand(Command.Execute, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void Stop()
    {
        Error err = CoreDoCommand(Command.Stop, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void Pause()
    {
        Error err = CoreDoCommand(Command.Pause, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void Resume()
    {
        Error err = CoreDoCommand(Command.Resume, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public int CoreStateQuery(CoreParam param)
    {
        int res = 0;
        Error err = CoreDoCommand(Command.CoreStateQuery, (int) param, ref res);
        ThrowForError(err);
        return res;
    }

    public int CoreStateQuery(CoreParam param, ref int res)
    {
        Error err = CoreDoCommand(Command.CoreStateQuery, (int) param, ref res);
        ThrowForError(err);
        return res;
    }

    public void CoreStateSet(CoreParam param, int value)
    {
        Error err = CoreDoCommand(Command.CoreStateSet, (int) param, ref value);
        ThrowForError(err);
    }

    public void LoadStateFromFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        Error err = CoreDoCommand(Command.StateLoad, 0, Path.GetFullPath(path));
        ThrowForError(err);
    }

    public void LoadStateFromCurrentSlot()
    {
        Error err = CoreDoCommand(Command.StateLoad, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void SaveStateToFile(string path, SavestateType type = SavestateType.Mupen64Plus)
    {
        ArgumentNullException.ThrowIfNull(path);
        Error err = CoreDoCommand(Command.StateSave, (int) type, Path.GetFullPath(path));
        ThrowForError(err);
    }

    public void SaveStateToCurrentSlot()
    {
        Error err = CoreDoCommand(Command.StateSave, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void SetSavestateSlot(int slot)
    {
        if (slot < 0 || slot > 9)
            throw new ArgumentOutOfRangeException(nameof(slot), "Savestate slots range from 0-9 (inclusive)");
        Error err = CoreDoCommand(Command.StateSetSlot, slot, IntPtr.Zero);
        ThrowForError(err);
    }

    public void Reset(bool hard = true)
    {
        Error err = CoreDoCommand(Command.Reset, hard ? 1 : 0, IntPtr.Zero);
        ThrowForError(err);
    }

    public void AdvanceFrame()
    {
        Error err = CoreDoCommand(Command.AdvanceFrame, 0, IntPtr.Zero);
        ThrowForError(err);
    }

    #endregion

    public void OverrideVideoExtensions(IVideoExtension obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        _vidextObject = obj;
        _vidextDelegates = new(obj);

        Error err = _fnCoreOverrideVidExt(_vidextDelegates!.AsNative());
        ThrowForError(err);
    }

    public void AttachPlugin(string path)
    {
        IntPtr pluginLib = NativeLibrary.Load(path);
        // Implicitly
        var getVersion = NativeLibHelper.GetFunction<DPluginGetVersion>(pluginLib, "PluginGetVersion");
        Error err = getVersion(out var type, out _, out _, out _, out _);

        if (!_pluginDict.TryAdd(type, pluginLib))
        {
            IntPtr oldLib = _pluginDict[type];
            var oldLibGetVersion = NativeLibHelper.GetFunction<DPluginGetVersion>(pluginLib, "PluginGetVersion");

            err = oldLibGetVersion(out _, out _, out _, out var oldName, out _);
            ThrowForError(err);
            
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

    public void DetachPlugin(PluginType type)
    {
        if (!_pluginDict.Remove(type, out IntPtr pluginLib))
            throw new InvalidOperationException($"Plugin type {type} does not have a plugin registered");

        Error err = _fnCoreDetachPlugin(type);
        ThrowForError(err);

        var shutdown = NativeLibHelper.GetFunction<DPluginShutdown>(pluginLib, "PluginShutdown");
        err = shutdown();
        ThrowForError(err);
        
        NativeLibrary.Free(pluginLib);
    }

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

        return Path.Join(new[] { path, "Libs", asLib("mupen64plus") });
    }

    private readonly IntPtr _libHandle;
}