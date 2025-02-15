using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using M64RPFW.Models.Emulation.Helpers;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;
using Silk.NET.SDL;

namespace M64RPFW.Models.Emulation;

public static partial class Mupen64Plus
{
    private static readonly DebugCallback _debugCallback;
    private static readonly StateCallback _stateCallback;
    private static readonly FrameCallback _frameCallback;
    private static readonly VCRMsgFunc _vcrMsgFunc;
    private static readonly VCRStateCallback _vcrStateCallback;

    private static IntPtr _libHandle;

    // Public API
    // ========================================================
#pragma warning disable CS8618, CS8602
    static unsafe Mupen64Plus()
    {
        string expectedPath = GetBundledLibraryPath();
        _libHandle = NativeLibrary.Load(Path.Join(expectedPath, NativeLibHelper.AsDLL("mupen64plus")));

        ResolveFrontendFunctions();
        ResolveConfigFunctions();
        ResolveVcrFunctions();
        ResolveEncoderFunctions();
        ResolveRdramFunctions();

        #region Install frontend callbacks

        _debugCallback = (context, level, message) =>
        {
            int messageLen = 0;
            while (message[messageLen] != 0) messageLen++;
            OnLogMessage(context, level, Encoding.ASCII.GetString(message, messageLen));
        };
        _stateCallback = OnStateChange;
        _vcrMsgFunc = (lvl, msg) =>
        {
            OnLogMessage((nint) (int) LogSources.VCR, lvl, msg);
            return true;
        };
        _vcrStateCallback = OnVCRStateChange;

        Mupen64PlusTypes.Error err = _fnCoreStartup(
            0x020000, null, expectedPath, (nint) (int) Mupen64PlusTypes.PluginType.Core,
            _debugCallback, IntPtr.Zero, _stateCallback);
        ThrowForError(err);

        _vcrSetErrorCallback(_vcrMsgFunc);
        _vcrSetStateCallback(_vcrStateCallback);

        _frameCallback = OnFrameComplete;
        err = _fnCoreDoCommand(Mupen64PlusTypes.Command.SetFrameCallback, 0,
            Marshal.GetFunctionPointerForDelegate(_frameCallback).ToPointer());
        ThrowForError(err);

        #endregion

        #region Install encoder callbacks

        _sampleCallback = OnSample;
        _rateChangedCallback = OnRateChanged;

        err = _encoderSetSampleCallback(_sampleCallback);
        ThrowForError(err);
        err = _encoderSetRateChangedCallback(_rateChangedCallback);
        ThrowForError(err);

        #endregion

        _pluginDict = new Dictionary<Mupen64PlusTypes.PluginType, IntPtr>();

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {

            ConfigSaveFile();

            // ReSharper disable once VariableHidesOuterVariable
            Mupen64PlusTypes.Error err = _fnCoreShutdown!();
            ThrowForError(err);

            NativeLibrary.Free(_libHandle);
        };
    }

    public static unsafe string GetErrorMessage(Mupen64PlusTypes.Error err)
    {
        byte* str = _fnCoreErrorMessage(err);
        return CHelpers.DecodeString(str);
    }

    private static void ThrowForError(Mupen64PlusTypes.Error err)
    {
        if (err == Mupen64PlusTypes.Error.Success)
            return;

        throw new MupenException(err);
    }

    private static void OnLogMessage(IntPtr context, Mupen64PlusTypes.MessageLevel level, string message)
    {
        var type = (int) context;

        string sourceString = type < 0x4000 ? "M64+" : "RPFW";

        string typeString = type switch
        {
            (int) Mupen64PlusTypes.PluginType.Core => "CORE ",
            (int) Mupen64PlusTypes.PluginType.Graphics => "VIDEO",
            (int) Mupen64PlusTypes.PluginType.Audio => "AUDIO",
            (int) Mupen64PlusTypes.PluginType.Input => "INPUT",
            (int) Mupen64PlusTypes.PluginType.RSP => "RSP  ",
            (int) LogSources.VCR => "VCR  ",
            (int) LogSources.App => "APP  ",
            (int) LogSources.Vidext => "VIDXT",
            (int) LogSources.Config => "CONF ",
            _ => "??   "
        };

        string levelString = level switch
        {
            Mupen64PlusTypes.MessageLevel.Error => "ERROR",
            Mupen64PlusTypes.MessageLevel.Warning => "WARN ",
            Mupen64PlusTypes.MessageLevel.Info => "INFO ",
            Mupen64PlusTypes.MessageLevel.Status => "STAT ",
            Mupen64PlusTypes.MessageLevel.Verbose => "TRACE",
            _ => "??   "
        };
        #if true
        Console.WriteLine($"[{sourceString} {typeString} {levelString}] {message}");
        #else
        Debug.WriteLine($"[{sourceString} {typeString} {levelString}] {message}");
        #endif
    }

    private static void OnStateChange(IntPtr context, Mupen64PlusTypes.CoreParam param, int newValue)
    {
        StateChanged?.Invoke(new StateChangeEventArgs
        {
            Param = param,
            NewValue = newValue
        });
    }

    private static void OnVCRStateChange(Mupen64PlusTypes.VCRParam param, int newValue)
    {
        VCRStateChanged?.Invoke(new VCRStateChangeEventArgs
        {
            Param = param,
            NewValue = newValue
        });
    }

    private static void OnFrameComplete(int frameIndex)
    {
        FrameComplete?.Invoke(frameIndex);
    }

#pragma warning restore CS8618

    public class StateChangeEventArgs : EventArgs
    {
        public Mupen64PlusTypes.CoreParam Param { get; init; }
        public int NewValue { get; init; }
    }

    public class VCRStateChangeEventArgs : EventArgs
    {
        public Mupen64PlusTypes.VCRParam Param { get; init; }
        public int NewValue { get; init; }
    }

    public static event Action<StateChangeEventArgs>? StateChanged;

    public static event Action<VCRStateChangeEventArgs>? VCRStateChanged;
    public static event Action<int>? FrameComplete;

    #region Core Commands

    /// <summary>
    /// Loads a ROM from the given path to the core. This does not support zipped ROMs.
    /// </summary>
    /// <param name="path">The path to the ROM file.</param>
    public static unsafe void OpenRom(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        byte[] bytes = File.ReadAllBytes(path);
        RomHelper.AdaptiveByteSwap(bytes);

        fixed (byte* bytesPtr = bytes)
        {
            Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.RomOpen, bytes.Length, bytesPtr);
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
            Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.RomOpen, romData.Length, dataPtr);
            ThrowForError(err);
        }
    }

    /// <summary>
    /// Close the currently open ROM.
    /// </summary>
    public static unsafe void CloseRom()
    {
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.RomClose, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Get the ROM header of the currently open ROM.
    /// </summary>
    /// <returns>the ROM header of the currently open ROM</returns>
    public static unsafe Mupen64PlusTypes.RomHeader GetRomHeader()
    {
        int size = Marshal.SizeOf<Mupen64PlusTypes.RomHeader>();
        IntPtr alloc = Marshal.AllocHGlobal(size);
        try
        {
            Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.RomGetHeader, size, alloc.ToPointer());
            ThrowForError(err);
            var res = Marshal.PtrToStructure<Mupen64PlusTypes.RomHeader>(alloc);
            return res;
        }
        finally
        {
            Marshal.FreeHGlobal(alloc);
        }
    }

    /// <summary>
    /// Get the ROM metadata for the currently open ROM.
    /// </summary>
    /// <returns>The ROM settings for the currently open ROM</returns>
    public static unsafe void GetRomSettings(out Mupen64PlusTypes.RomSettings settings)
    {
        fixed (Mupen64PlusTypes.RomSettings* pSettings = &settings)
        {
            Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.RomGetSettings, sizeof(Mupen64PlusTypes.RomSettings), pSettings);
            ThrowForError(err);
        }
    }

    /// <summary>
    /// Run the current ROM in this thread. This function blocks until the emulator is stopped.
    /// </summary>
    public static unsafe void Execute()
    {
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.Execute, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Stops the emulator. The emulator must not be stopped.
    /// </summary>
    public static unsafe void Stop()
    {
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.Stop, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Pauses the emulator. The emulator must be running.
    /// </summary>
    public static unsafe void Pause()
    {
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.Pause, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Resumes the emulator. The emulator must be paused.
    /// </summary>
    public static unsafe void Resume()
    {
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.Resume, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Gets one of the emulator parameters.
    /// </summary>
    /// <param name="param">the parameter to query</param>
    /// <returns>the parameter's value</returns>
    public static unsafe int CoreStateQuery(Mupen64PlusTypes.CoreParam param)
    {
        int res = 0;
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.CoreStateQuery, (int) param, &res);
        ThrowForError(err);

        return res;
    }

    /// <summary>
    /// Sets one of the emulator parameters.
    /// </summary>
    /// <param name="param">the parameter to set</param>
    /// <param name="value">the value to set to the parameter</param>
    public static unsafe void CoreStateSet(Mupen64PlusTypes.CoreParam param, int value)
    {
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.CoreStateSet, (int) param, &value);
        ThrowForError(err);
    }

    public static unsafe void CoreStateSet(Mupen64PlusTypes.CoreParam param, uint value)
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
            Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.StateLoad, 0, alloc.ToPointer());
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
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.StateLoad, 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Saves the emulator's state as a file.
    /// </summary>
    /// <param name="path">The path to save the savestate</param>
    /// <param name="type"></param>
    public static unsafe void SaveStateToFile(string path, Mupen64PlusTypes.SavestateType type = Mupen64PlusTypes.SavestateType.Mupen64Plus)
    {
        ArgumentNullException.ThrowIfNull(path);

        string fullPath = Path.GetFullPath(path);
        IntPtr alloc = Marshal.StringToHGlobalAnsi(fullPath);
        try
        {
            Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.StateSave, (int) type, alloc.ToPointer());
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
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.StateSave, 0, null);
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
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.StateSetSlot, slot, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Resets the emulator.
    /// </summary>
    /// <param name="hard">If true, performs a hard reset. Otherwise, performs a soft reset.</param>
    public static unsafe void Reset(bool hard = true)
    {
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.Reset, hard ? 1 : 0, null);
        ThrowForError(err);
    }

    /// <summary>
    /// Advances the emulator by a single frame.
    /// </summary>
    public static unsafe void AdvanceFrame()
    {
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.AdvanceFrame, 0, null);
        ThrowForError(err);
    }

    public static unsafe void SendSDLKeyDown(Scancode scancode, Keymod modifiers)
    {
        uint combined = (uint) modifiers << 16 | (uint) scancode;
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.SendSDLKeyDown, (int) combined, null);
        ThrowForError(err);
    }

    public static unsafe void SendSDLKeyUp(Scancode scancode, Keymod modifiers)
    {
        uint combined = (uint) modifiers << 16 | (uint) scancode;
        Mupen64PlusTypes.Error err = _fnCoreDoCommand(Mupen64PlusTypes.Command.SendSDLKeyUp, (int) combined, null);
        ThrowForError(err);
    }

    #endregion

    #region Miscellaneous core functions

    /// <summary>
    /// Get the ROM metadata for a given CRC pair.
    /// </summary>
    /// <param name="settings">The output object.</param>
    /// <param name="crc1">The first CRC.</param>
    /// <param name="crc2">The second CRC.</param>
    public static unsafe void GetRomSettings(out Mupen64PlusTypes.RomSettings settings, uint crc1, uint crc2)
    {
        fixed (Mupen64PlusTypes.RomSettings* pSettings = &settings)
        {
            var err = _fnCoreGetRomSettings(pSettings, sizeof(Mupen64PlusTypes.RomSettings), (int) crc1, (int) crc2);
            ThrowForError(err);
        }
    }

    /// <summary>
    /// Overrides the core "video extension" functions. These handle window
    /// management for the video plugin.
    /// </summary>
    /// <param name="vidext">The video extension functions to use, or null to remove it.</param>
    public static void OverrideVidExt(Mupen64PlusTypes.VideoExtensionFunctions? vidext)
    {
        _currentVidext = vidext;
        Mupen64PlusTypes.Error err = _fnCoreOverrideVidExt(vidext ?? Mupen64PlusTypes.VideoExtensionFunctions.Empty);
        ThrowForError(err);
    }

    private static Mupen64PlusTypes.VideoExtensionFunctions? _currentVidext;

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
    /// <param name="intendedType">The intended plugin type. If Null, it will be inferred from the plugin's type.</param>
    /// <param name="preStartup">A function to execute before calling PluginStartup().</param>
    /// <exception cref="InvalidOperationException">If the located plugin's type already has an attached plugin</exception>
    /// <exception cref="FileNotFoundException">If the located plugin doesn't exist, can't be loaded, or is the wrong type.</exception>
    public static unsafe void AttachPlugin(string path, Mupen64PlusTypes.PluginType intendedType = Mupen64PlusTypes.PluginType.Null, Action<IntPtr>? preStartup = null)
    {
        IntPtr pluginLib = IntPtr.Zero;
        DPluginGetVersion? getVersion;
        bool didStartup = false;
        try
        {
            // Load the plugin
            try
            {
                pluginLib = NativeLibrary.Load(path);
                getVersion = NativeLibHelper.GetFunction<DPluginGetVersion>(pluginLib, "PluginGetVersion");

            }
            catch (DllNotFoundException e)
            {
                throw new FileNotFoundException($"{path} does not specify a shared library", e);
            }
            catch (Exception e) when (e is EntryPointNotFoundException or
                                          ArgumentNullException or
                                          BadImageFormatException)
            {
                throw new FileNotFoundException($"{path} is not a valid plugin", e);
            }

            // Check its type and ensure it's what we need
            Mupen64PlusTypes.Error err = getVersion(out var type, out _, out _, out _, out _);
            ThrowForError(err);

            if (intendedType != Mupen64PlusTypes.PluginType.Null && type != intendedType)
            {
                throw new FileNotFoundException($"Expected a plugin of type {intendedType}, instead got {type}");
            }

            if (!_pluginDict.TryAdd(type, pluginLib))
            {
                IntPtr oldLib = _pluginDict[type];
                var oldLibGetVersion = NativeLibHelper.GetFunction<DPluginGetVersion>(pluginLib, "PluginGetVersion");

                err = oldLibGetVersion(out _, out _, out _, out byte* oldNameBytes, out _);
                ThrowForError(err);

                string oldName = CHelpers.DecodeString(oldNameBytes);

                throw new InvalidOperationException(
                    $"Plugin type {type} already has a plugin registered ({oldName})");
            }
            // Start up the plugin and attach it
            preStartup?.Invoke(pluginLib);
            var startup = NativeLibHelper.GetFunction<DPluginStartup>(pluginLib, "PluginStartup");
            err = startup(_libHandle, (IntPtr) (int) type, _debugCallback);
            ThrowForError(err);
            didStartup = true;

            err = _fnCoreAttachPlugin(type, pluginLib);
            ThrowForError(err);
        }
        catch
        {
            if (didStartup)
                NativeLibHelper.GetFunction<DPluginShutdown>(pluginLib, "PluginShutdown")();
            NativeLibrary.Free(pluginLib);
            throw;
        }
    }

    /// <summary>
    /// Detaches a plugin from the core. Unlike <see cref="AttachPlugin"/>, does not mandate
    /// any sort of order.
    /// </summary>
    /// <param name="type">The plugin type to detach</param>
    public static void DetachPlugin(Mupen64PlusTypes.PluginType type)
    {
        if (!_pluginDict.Remove(type, out var pluginLib))
            return;
        Mupen64PlusTypes.Error err = _fnCoreDetachPlugin(type);
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
        Mupen64PlusTypes.Error err = _fnCorePluginGetVersion(out _, out var version, out var apiVersion, out var name, out _);
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
        VCR = 0x2000,
        App = 0x4000,
        Vidext,
        Config
    }

    public static void Log(LogSources source, Mupen64PlusTypes.MessageLevel level, string message, params object[] args)
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

        return Path.Join(new[]
        {
            path,
            "Libraries"
        });
    }

}