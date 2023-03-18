using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Mupen64PlusRR.Models.Helpers;

namespace Mupen64PlusRR.Models.Emulation;

public static partial class Mupen64Plus
{
    
    private static readonly DebugCallback _debugCallback;
    private static readonly StateCallback _stateCallback;
    private static readonly FrameCallback _frameCallback;

    private static GCHandle[] _callbackGCHandles;
    
    // Public API
    // ========================================================
#pragma warning disable CS8618, CS8602
    static unsafe Mupen64Plus()
    {
        string expectedPath = GetBundledLibraryPath();
        _libHandle = NativeLibrary.Load(Path.Join(expectedPath, NativeLibHelper.AsDLL("mupen64plus")));

        ResolveFrontendFunctions();
        ResolveConfigFunctions();

        _debugCallback = OnLogMessage;
        _stateCallback = OnStateChange;

        Error err = _fnCoreStartup(
            0x020000, null, expectedPath, (IntPtr) (int) PluginType.Core,
            _debugCallback, IntPtr.Zero, _stateCallback);
        ThrowForError(err);


        _frameCallback = OnFrameComplete;
        err = _fnCoreDoCommand(Command.SetFrameCallback, 0,
            Marshal.GetFunctionPointerForDelegate(_frameCallback).ToPointer());
        ThrowForError(err);

        _callbackGCHandles = new[]
        {
            GCHandle.Alloc(_debugCallback, GCHandleType.Normal),
            GCHandle.Alloc(_stateCallback, GCHandleType.Normal),
            GCHandle.Alloc(_frameCallback, GCHandleType.Normal),
        };

        _pluginDict = new Dictionary<PluginType, IntPtr>();

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            foreach (var handle in _callbackGCHandles)
                handle.Free();
            
            ConfigSaveFile();

            // ReSharper disable once VariableHidesOuterVariable
            Error err = _fnCoreShutdown!();
            ThrowForError(err);

            NativeLibrary.Free(_libHandle);
        };
    }

    private static void ThrowForError(Error err)
    {
        if (err == Error.Success)
            return;

        var errType = err switch
        {
            Error.NotInit => typeof(InvalidOperationException),
            Error.AlreadyInit => typeof(InvalidOperationException),
            Error.InvalidState => typeof(InvalidOperationException),
            Error.Incompatible => typeof(InvalidOperationException),
            Error.InputAssert => typeof(ArgumentException),
            Error.InputInvalid => typeof(ArgumentException),
            Error.InputNotFound => typeof(ArgumentException),
            Error.WrongType => typeof(ArgumentException),
            Error.PluginFail => typeof(ApplicationException),
            Error.SystemFail => typeof(ApplicationException),
            Error.Internal => typeof(ApplicationException),
            Error.Files => typeof(IOException),
            Error.NoMemory => typeof(OutOfMemoryException),
            _ => typeof(ApplicationException)
        };
        throw (Exception) Activator.CreateInstance(errType, $"M64+ {_fnCoreErrorMessage(err)}")!;
    }

    private static void OnLogMessage(IntPtr context, MessageLevel level, string message)
    {
        var type = (int) context;

        string typeString = type switch
        {
            (int) PluginType.Core => "CORE  ",
            (int) PluginType.Graphics => "VIDEO ",
            (int) PluginType.Audio => "AUDIO ",
            (int) PluginType.Input => "INPUT ",
            (int) PluginType.RSP => "RSP   ",
            (int) LogSources.App => "APP   ",
            (int) LogSources.Vidext => "VIDXT ",
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
    public static unsafe void OpenRomBinary(ReadOnlySpan<byte> romData)
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

    public static unsafe void CoreStateSet(CoreParam param, uint value)
    {
        CoreStateSet(param, (int) value);
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

    public static unsafe void SendSDLKeyDown(uint combined)
    {
        Error err = _fnCoreDoCommand(Command.SendSDLKeyDown, (int) combined, null);
        ThrowForError(err);
    }

    public static unsafe void SendSDLKeyUp(uint combined)
    {
        Error err = _fnCoreDoCommand(Command.SendSDLKeyUp, (int) combined, null);
        ThrowForError(err);
    }

    #endregion

    #region Miscellaneous core functions
    
    /// <summary>
    /// Overrides the core "video extension" functions. These handle window
    /// management for the video plugin.
    /// </summary>
    /// <param name="vidext">The video extension functions to use, or null to remove it.</param>
    public static void OverrideVidExt(VideoExtensionFunctions? vidext)
    {
        _currentVidext = vidext;
        Error err = _fnCoreOverrideVidExt(vidext ?? VideoExtensionFunctions.Empty);
        ThrowForError(err);
    }

    private static VideoExtensionFunctions? _currentVidext;

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
        err = startup(_libHandle, (IntPtr) (int) type, OnLogMessage);
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
        if (!_pluginDict.Remove(type, out var pluginLib))
            return;
        Error err = _fnCoreDetachPlugin(type);
        ThrowForError(err);

        var shutdown = NativeLibHelper.GetFunction<DPluginShutdown>(pluginLib, "PluginShutdown");
        err = shutdown();
        ThrowForError(err);

        NativeLibrary.Free(pluginLib);
    }

    public struct CoreVersion
    {
        public uint Version;
        public uint APIVersion;
        public string Name;
        public uint VersionMajor => (Version >> 16) & 0xFF;
        public uint VersionMinor => (Version >> 8) & 0xFF;
        public uint VersionPatch => (Version >> 0) & 0xFF;
    }

    public static unsafe CoreVersion GetVersionInfo()
    {
        Error err = _fnCorePluginGetVersion(out _, out var version, out var apiVersion, out var name, out _);
        ThrowForError(err);

        return new CoreVersion
        {
            Version = (uint) version,
            APIVersion = (uint) apiVersion,
            Name = Marshal.PtrToStringAnsi((IntPtr) name) ?? ""
        };
    }

    #endregion

    #region Logging

    public enum LogSources
    {
        App = 0x4201,
        Vidext,
    }

    public static void Log(LogSources source, MessageLevel level, string message, params object[] args)
    {
        OnLogMessage((IntPtr) source, level, string.Format(message, args));
    }

    #endregion

    // Utilities
    // =================================
    public static string GetBundledLibraryPath()
    {
        string path = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) ??
                      throw new ApplicationException("Could not retrieve .exe path");

        return Path.Join(new[] { path, "Libraries" });
    }

    private static IntPtr _libHandle;
}