using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using M64RPFW.Models.Emulation.API.Plugins;
using M64RPFW.Models.Emulation.Exceptions;
using M64RPFW.Services;
using static M64RPFW.Models.Emulation.API.Mupen64PlusDelegates;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.API;

public sealed class Mupen64PlusApi : IDisposable
{
    internal Mupen64PlusApi(IFilesService filesService)
    {
        this._filesService = filesService;
    }

    #region Other

    public void Dispose()
    {
        // this is called on non-emu thread because emu thread is stuck inside m64p
        // so dont do anything but sending close command
        if (!_disposed)
            while (_isBusyInCore)
                // send command multiple times to assure it closes :/
                _ = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdStop, 0, IntPtr.Zero);
    }

    #endregion

    private void GetScreenDimensions(out int width, out int height)
    {
        int w = 0, h = 0;
        _videoPlugin.ReadScreen2Res(IntPtr.Zero, ref w, ref h, 0);
        width = w;
        height = h;
    }

    private void AllocateFrameBuffer(int width, int height)
    {
        BufferWidth = width;
        BufferHeight = height;

        FrameBuffer = new int[BufferWidth * BufferHeight];

        if (IsFrameBufferInitialized) // sometimes it randomly returns 0,0
            OnFrameBufferCreate?.Invoke();
    }

    private void UpdateFramebuffer()
    {
        GetScreenDimensions(out var newWidth, out var newHeight);

        if (newWidth != BufferWidth || newHeight != BufferHeight) AllocateFrameBuffer(newWidth, newHeight);

        if (FrameBuffer == null)
        {
            // ??? something's wrong
            Debug.Print("Framebuffer is null after size change check");
            AllocateFrameBuffer(2, 2);
        }

        var frameBuffer = FrameBuffer;
        int bufferWidth = BufferWidth, bufferHeight = BufferHeight;
        _videoPlugin.ReadScreen2(frameBuffer, ref bufferWidth, ref bufferHeight, 0);
        FrameBuffer = frameBuffer;
        BufferWidth = bufferWidth;
        BufferHeight = bufferHeight;
    }

    private void SetSetting(string section, string name, object value, bool save = false)
    {
        Debug.Print($"{section} {name} - {value}");

        var sectionPtr = IntPtr.Zero;

        _ = _corePlugin.ConfigOpenSection(section, ref sectionPtr);

        _ = value is int intValue
            ? _corePlugin.ConfigSetParameterInt(sectionPtr, name, EmulatorTypes.M64TypeInt, ref intValue)
            : value is bool boolValue
                ? _corePlugin.ConfigSetParameterBool(sectionPtr, name, EmulatorTypes.M64TypeBool, ref boolValue)
                : value is string stringValue
                    ? _corePlugin.ConfigSetParameterString(sectionPtr, name, EmulatorTypes.M64TypeString,
                        new StringBuilder(stringValue))
                    : throw new UnresolvableConfigEntryTypeException(
                        $"Type {value.GetType()} could not be resolved to a type accepted by m64p+");

        if (save) _ = _corePlugin.ConfigSaveFile();
    }

    private void ApplyConfig(Mupen64PlusConfig config)
    {
        foreach (var item in config.GetType().GetFields())
        {
            var attrib = item.GetCustomAttribute<Mupen64PlusConfigEntryAttribute>();
            SetSetting(attrib.Section, attrib.Name, item.GetValue(config));
        }

        _ = _corePlugin.ConfigSaveFile();
    }

    /// <summary>
    ///     Launch
    /// </summary>
    public void Launch(Mupen64PlusLaunchParameters launchParameters)
    {
        if (_isBusyInCore) throw new EmulatorAlreadyRunningException();
        _disposed = false;

        using (_corePlugin = new CorePlugin(EmulatorPluginType.M64PluginCore,
                   NativeLibrary.Load(launchParameters.CoreLibrary.Path)))
        using (_videoPlugin = new VideoPlugin(EmulatorPluginType.M64PluginGfx,
                   NativeLibrary.Load(launchParameters.VideoPlugin.Path)))
        using (_audioPlugin = new AudioPlugin(EmulatorPluginType.M64PluginAudio,
                   NativeLibrary.Load(launchParameters.AudioPlugin.Path)))
        using (_inputPlugin = new InputPlugin(EmulatorPluginType.M64PluginInput,
                   NativeLibrary.Load(launchParameters.InputPlugin.Path)))
        using (_rspPlugin = new RspPlugin(EmulatorPluginType.M64PluginRsp,
                   NativeLibrary.Load(launchParameters.RspPlugin.Path)))
        {
            _corePlugin.Attach(null);


            EmulatorStatus result;

            result = _corePlugin.Startup(
                0x20001,
                "",
                "",
                "Core",
                null,
                "",
                IntPtr.Zero
            );

            ApplyConfig(launchParameters.Config);

            result = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdStateSetSlot, launchParameters.InitialSlot,
                IntPtr.Zero);

            result = _corePlugin.DoCommandByteArray(EmulatorCommand.M64CmdRomOpen, launchParameters.Rom.Length,
                launchParameters.Rom);
            var sizeHeader = Marshal.SizeOf(typeof(EmulatorRomHeader));

            _videoPlugin.Attach(_corePlugin);
            _audioPlugin.Attach(_corePlugin);
            _inputPlugin.Attach(_corePlugin);
            _rspPlugin.Attach(_corePlugin);

            result = _corePlugin.DoCommandFrameCallback(EmulatorCommand.M64CmdSetFrameCallback, 0, delegate
            {
                Debug.Print("New frame");
                UpdateFramebuffer();
                OnFrameFinish?.Invoke();
                if (IsFrameBufferInitialized) OnFrameBufferUpdate?.Invoke();
            });

            result = _corePlugin.DoCommandViCallback(EmulatorCommand.M64CmdSetViCallback, 0,
                delegate { OnViInterrupt?.Invoke(); });

            result = _corePlugin.DoCommandRenderCallback(EmulatorCommand.M64CmdSetRenderCallback, 0,
                delegate { OnPostRender?.Invoke(); });

            var enc = (launchParameters.Config.ScreenWidth << 16) + launchParameters.Config.ScreenHeight;
            result = _corePlugin.DoCommandCoreStateSet(
                EmulatorCommand.M64CmdCoreStateSet,
                EmulatorCoreParameters.M64CoreVideoSize,
                ref enc
            );

            ExecuteEmulator();

            _videoPlugin.Detach(_corePlugin);
            _audioPlugin.Detach(_corePlugin);
            _inputPlugin.Detach(_corePlugin);
            _rspPlugin.Detach(_corePlugin);

            _ = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdRomClose, 0, IntPtr.Zero);

            _ = _corePlugin.Shutdown();
        }
    }

    /// <summary>
    ///     Starts execution of mupen64plus
    ///     Does not return until the emulator stops
    /// </summary>
    private void ExecuteEmulator()
    {
        _isBusyInCore = true;

        StartupCallbackDelegate cb = () => _m64PStartupComplete.Set();
        _ = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdExecute, 0, Marshal.GetFunctionPointerForDelegate(cb));

        _isBusyInCore = false;

        Debug.Print("Escaped core code");
    }

    #region Events

    public event Action OnFrameFinish;
    public event Action OnViInterrupt;
    public event Action OnPostRender;

    public event Action OnFrameBufferCreate;
    public event Action OnFrameBufferUpdate;

    #endregion

    #region Private Fields

    private bool _disposed;
    private readonly ManualResetEvent _m64PStartupComplete = new(false);
    private readonly Task? _m64PEmulator;
    private readonly IFilesService _filesService;
    private volatile bool _isBusyInCore;

    private CorePlugin _corePlugin;
    private VideoPlugin _videoPlugin;
    private AudioPlugin _audioPlugin;
    private InputPlugin _inputPlugin;
    private RspPlugin _rspPlugin;

    #endregion

    #region Properties

    public bool IsEmulatorRunning => _isBusyInCore;
    public bool IsFrameBufferInitialized => FrameBuffer != null && BufferWidth != 0 && BufferHeight != 0;
    public int[]? FrameBuffer { get; private set; }
    public int BufferWidth { get; private set; }
    public int BufferHeight { get; private set; }

    #endregion

    #region Interaction Functions

    public void SetStateSlot(int slot)
    {
        _ = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdStateSetSlot, slot, IntPtr.Zero);
    }

    public EmulatorRomHeader GetRomHeader(EmulatorRomHeader romHeader)
    {
        var size = Marshal.SizeOf(typeof(EmulatorRomHeader));

        _ = _corePlugin.DoCommandRomHeader(EmulatorCommand.M64CmdRomGetHeader, size, ref romHeader);

        return romHeader;
    }

    public EmulatorRomSettings GetRomSettings(EmulatorRomSettings romSettings)
    {
        var size = Marshal.SizeOf(typeof(EmulatorRomSettings));

        _ = _corePlugin.DoCommandRomSettings(EmulatorCommand.M64CmdRomGetSettings, romSettings, ref size);

        return romSettings;
    }

    public void LoadState(int slot)
    {
        _ = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdStateLoad, slot, IntPtr.Zero);
    }

    public void SaveState(int slot)
    {
        _ = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdStateSave, slot, IntPtr.Zero);
    }

    public void LoadState(string filePath)
    {
        _ = _corePlugin.DoCommandString(EmulatorCommand.M64CmdStateLoad, 0, filePath);
    }

    public void SaveState(string filePath, SaveStateTypes type = SaveStateTypes.Mupen64Plus)
    {
        _ = _corePlugin.DoCommandString(EmulatorCommand.M64CmdStateSave, (int)type, filePath);
    }

    public void Screenshot()
    {
        _ = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdTakeNextScreenshot, 0, IntPtr.Zero);
    }

    public void SetPlayMode(PlayModes playMode)
    {
        var dummy = (int)playMode;
        _ = _corePlugin.DoCommandCoreStateSet(
            EmulatorCommand.M64CmdCoreStateSet,
            EmulatorCoreParameters.M64CoreEmuState,
            ref dummy
        );
    }

    public void SetSoundMode(bool silence)
    {
        var dummy = silence ? 1 : 0;
        _ = _corePlugin.DoCommandCoreStateSet(
            EmulatorCommand.M64CmdCoreStateSet,
            EmulatorCoreParameters.M64CoreAudioMute,
            ref dummy
        );
    }

    public void Stop()
    {
        Dispose();
    }

    public void Reset(bool soft)
    {
        _ = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdReset, soft ? 0 : 1, IntPtr.Zero);
    }

    public void SetSpeedFactor(int speed)
    {
        _ = _corePlugin.DoCommandCoreStateSet(
            EmulatorCommand.M64CmdCoreStateSet,
            EmulatorCoreParameters.M64CoreSpeedFactor,
            ref speed
        );
    }

    public void SetVideoMode(EmulatorVideoModes mode)
    {
        var dummy = (int)mode;
        _ = _corePlugin.DoCommandCoreStateSet(
            EmulatorCommand.M64CmdCoreStateSet,
            EmulatorCoreParameters.M64CoreVideoMode,
            ref dummy);
    }


    public void FrameAdvance()
    {
        _ = _corePlugin.DoCommandPointer(EmulatorCommand.M64CmdAdvanceFrame, 0, IntPtr.Zero);
    }

    #endregion
}