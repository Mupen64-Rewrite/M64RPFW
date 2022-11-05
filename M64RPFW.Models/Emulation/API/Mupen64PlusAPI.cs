using M64RPFW.Models.Emulation.API.Plugins;
using M64RPFW.Models.Emulation.Exceptions;
using M64RPFW.Services;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static M64RPFW.Models.Emulation.API.Mupen64PlusDelegates;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.API
{
    public sealed class Mupen64PlusAPI : IDisposable
    {
        #region Events

        public event Action OnFrameFinish;
        public event Action OnVIInterrupt;
        public event Action OnPostRender;

        public event Action OnFrameBufferCreate;
        public event Action OnFrameBufferUpdate;

        #endregion

        #region Private Fields

        private bool disposed = false;
        private readonly ManualResetEvent m64pStartupComplete = new(false);
        private readonly Task? m64pEmulator;
        private readonly IFilesService filesService;
        private volatile bool isBusyInCore;

        private CorePlugin corePlugin;
        private VideoPlugin videoPlugin;
        private AudioPlugin audioPlugin;
        private InputPlugin inputPlugin;
        private RspPlugin rspPlugin;

        #endregion

        #region Properties
        public bool IsEmulatorRunning => isBusyInCore;
        public bool IsFrameBufferInitialized => FrameBuffer != null && BufferWidth != 0 && BufferHeight != 0;
        public int[]? FrameBuffer { get; private set; }
        public int BufferWidth { get; private set; }
        public int BufferHeight { get; private set; }

        #endregion

        public Mupen64PlusAPI(IFilesService filesService)
        {
            this.filesService = filesService;
        }

        private void GetScreenDimensions(out int width, out int height)
        {
            int w = 0, h = 0;
            videoPlugin.ReadScreen2Res(IntPtr.Zero, ref w, ref h, 0);
            width = w;
            height = h;
        }

        private void AllocateFrameBuffer(int width, int height)
        {
            BufferWidth = width;
            BufferHeight = height;

            FrameBuffer = new int[BufferWidth * BufferHeight];

            if (IsFrameBufferInitialized) // sometimes it randomly returns 0,0
            {
                OnFrameBufferCreate?.Invoke();
            }
        }

        private void UpdateFramebuffer()
        {
            GetScreenDimensions(out int newWidth, out int newHeight);

            if (newWidth != BufferWidth || newHeight != BufferHeight)
            {
                AllocateFrameBuffer(newWidth, newHeight);
            }

            if (FrameBuffer == null)
            {
                // ??? something's wrong
                Debug.Print("Framebuffer is null after size change check");
                AllocateFrameBuffer(2, 2);
            }

            int[] frameBuffer = FrameBuffer;
            int bufferWidth = BufferWidth, bufferHeight = BufferHeight;
            videoPlugin.ReadScreen2(frameBuffer, ref bufferWidth, ref bufferHeight, 0);
            FrameBuffer = frameBuffer;
            BufferWidth = bufferWidth;
            BufferHeight = bufferHeight;
        }

        private void SetSetting(string section, string name, object value, bool save = false)
        {
            Debug.Print($"{section} {name} - {value}");

            IntPtr _section = IntPtr.Zero;

            _ = corePlugin.ConfigOpenSection(section, ref _section);

            _ = value is int intValue
                ? corePlugin.ConfigSetParameterInt(_section, name, EmulatorTypes.M64TYPE_INT, ref intValue)
                : value is bool boolValue
                    ? corePlugin.ConfigSetParameterBool(_section, name, EmulatorTypes.M64TYPE_BOOL, ref boolValue)
                    : value is string stringValue
                                    ? corePlugin.ConfigSetParameterString(_section, name, EmulatorTypes.M64TYPE_STRING, new StringBuilder(stringValue))
                                    : throw new UnresolvableConfigEntryTypeException($"Type {value.GetType()} could not be resolved to a type accepted by m64p+");

            if (save)
            {
                _ = corePlugin.ConfigSaveFile();
            }
        }

        private void ApplyConfig(Mupen64PlusConfig config)
        {
            foreach (FieldInfo item in config.GetType().GetFields())
            {
                Mupen64PlusConfigEntryAttribute? attrib = item.GetCustomAttribute<Mupen64PlusConfigEntryAttribute>();
                SetSetting(attrib.Section, attrib.Name, item.GetValue(config));
            }

            _ = corePlugin.ConfigSaveFile();
        }

        /// <summary>
        /// Launch
        /// </summary>
        public void Launch(Mupen64PlusLaunchParameters launchParameters)
        {
            if (isBusyInCore)
            {
                throw new EmulatorAlreadyRunningException();
            }
            disposed = false;

            using (corePlugin = new(EmulatorPluginType.M64PLUGIN_CORE, NativeLibrary.Load(launchParameters.CoreLibrary.Path)))
            using (videoPlugin = new(EmulatorPluginType.M64PLUGIN_GFX, NativeLibrary.Load(launchParameters.VideoPlugin.Path)))
            using (audioPlugin = new(EmulatorPluginType.M64PLUGIN_AUDIO, NativeLibrary.Load(launchParameters.AudioPlugin.Path)))
            using (inputPlugin = new(EmulatorPluginType.M64PLUGIN_INPUT, NativeLibrary.Load(launchParameters.InputPlugin.Path)))
            using (rspPlugin = new(EmulatorPluginType.M64PLUGIN_RSP, NativeLibrary.Load(launchParameters.RspPlugin.Path)))
            {
                corePlugin.Attach(null);


                EmulatorStatus result;

                result = corePlugin.Startup(
                    0x20001,
                    "",
                    "",
                    "Core",
                    null,
                    "",
                    IntPtr.Zero
                );

                ApplyConfig(launchParameters.Config);

                result = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_STATE_SET_SLOT, launchParameters.InitialSlot, IntPtr.Zero);

                result = corePlugin.DoCommandByteArray(EmulatorCommand.M64CMD_ROM_OPEN, launchParameters.Rom.Length, launchParameters.Rom);
                int sizeHeader = Marshal.SizeOf(typeof(EmulatorRomHeader));

                videoPlugin.Attach(corePlugin);
                audioPlugin.Attach(corePlugin);
                inputPlugin.Attach(corePlugin);
                rspPlugin.Attach(corePlugin);

                result = corePlugin.DoCommandFrameCallback(EmulatorCommand.M64CMD_SET_FRAME_CALLBACK, 0, new FrameCallbackDelegate(delegate
                {
                    Debug.Print("New frame");
                    UpdateFramebuffer();
                    OnFrameFinish?.Invoke();
                    if (IsFrameBufferInitialized)
                    {
                        OnFrameBufferUpdate?.Invoke();
                    }
                }));

                result = corePlugin.DoCommandVICallback(EmulatorCommand.M64CMD_SET_VI_CALLBACK, 0, new VICallbackDelegate(delegate
                {
                    OnVIInterrupt?.Invoke();
                }));

                result = corePlugin.DoCommandRenderCallback(EmulatorCommand.M64CMD_SET_RENDER_CALLBACK, 0, new RenderCallbackDelegate(delegate
                {
                    OnPostRender?.Invoke();
                }));

                int enc = (launchParameters.Config.ScreenWidth << 16) + launchParameters.Config.ScreenHeight;
                result = corePlugin.DoCommandCoreStateSet(
                        EmulatorCommand.M64CMD_CORE_STATE_SET,
                        EmulatorCoreParameters.M64CORE_VIDEO_SIZE,
                        ref enc
                    );

                ExecuteEmulator();

                videoPlugin.Detach(corePlugin);
                audioPlugin.Detach(corePlugin);
                inputPlugin.Detach(corePlugin);
                rspPlugin.Detach(corePlugin);

                _ = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_ROM_CLOSE, 0, IntPtr.Zero);

                _ = corePlugin.Shutdown();
            }

        }

        /// <summary>
        /// Starts execution of mupen64plus
        /// Does not return until the emulator stops
        /// </summary>
        private void ExecuteEmulator()
        {
            isBusyInCore = true;

            StartupCallbackDelegate cb = new(() => m64pStartupComplete.Set());
            _ = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_EXECUTE, 0, Marshal.GetFunctionPointerForDelegate(cb));

            isBusyInCore = false;

            Debug.Print("Escaped core code");
        }

        #region Interaction Functions

        public void SetStateSlot(int slot)
        {
            _ = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_STATE_SET_SLOT, slot, IntPtr.Zero);
        }

        public EmulatorRomHeader GetRomHeader(EmulatorRomHeader _rom_header)
        {
            int size = Marshal.SizeOf(typeof(EmulatorRomHeader));

            _ = corePlugin.DoCommandRomHeader(EmulatorCommand.M64CMD_ROM_GET_HEADER, size, ref _rom_header);

            return _rom_header;
        }

        public EmulatorRomSettings GetRomSettings(EmulatorRomSettings _rom_settings)
        {
            int size = Marshal.SizeOf(typeof(EmulatorRomSettings));

            _ = corePlugin.DoCommandRomSettings(EmulatorCommand.M64CMD_ROM_GET_SETTINGS, _rom_settings, ref size);

            return _rom_settings;
        }

        public void LoadState(int slot)
        {
            _ = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_STATE_LOAD, slot, IntPtr.Zero);
        }

        public void SaveState(int slot)
        {
            _ = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_STATE_SAVE, slot, IntPtr.Zero);
        }
        public void LoadState(string filePath)
        {
            _ = corePlugin.DoCommandString(EmulatorCommand.M64CMD_STATE_LOAD, 0, filePath);
        }

        public void SaveState(string filePath, SaveStateTypes type = SaveStateTypes.Mupen64Plus)
        {
            _ = corePlugin.DoCommandString(EmulatorCommand.M64CMD_STATE_SAVE, (int)type, filePath);
        }

        public void Screenshot()
        {
            _ = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_TAKE_NEXT_SCREENSHOT, 0, IntPtr.Zero);
        }

        public void SetPlayMode(PlayModes playMode)
        {
            int dummy = (int)playMode;
            _ = corePlugin.DoCommandCoreStateSet(
                    EmulatorCommand.M64CMD_CORE_STATE_SET,
                    EmulatorCoreParameters.M64CORE_EMU_STATE,
                    ref dummy
                );
        }

        public void SetSoundMode(bool silence)
        {
            int dummy = silence ? 1 : 0;
            _ = corePlugin.DoCommandCoreStateSet(
                EmulatorCommand.M64CMD_CORE_STATE_SET,
                EmulatorCoreParameters.M64CORE_AUDIO_MUTE,
                ref dummy
            );
        }

        public void Stop()
        {
            Dispose();
        }

        public void Reset(bool soft)
        {
            _ = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_RESET, soft ? 0 : 1, IntPtr.Zero);
        }

        public void SetSpeedFactor(int speed)
        {
            _ = corePlugin.DoCommandCoreStateSet(
                EmulatorCommand.M64CMD_CORE_STATE_SET,
                EmulatorCoreParameters.M64CORE_SPEED_FACTOR,
                ref speed
            );
        }

        public void SetVideoMode(EmulatorVideoModes mode)
        {
            int dummy = (int)mode;
            _ = corePlugin.DoCommandCoreStateSet(
                    EmulatorCommand.M64CMD_CORE_STATE_SET,
                    EmulatorCoreParameters.M64CORE_VIDEO_MODE,
                    ref dummy);
        }


        public void FrameAdvance()
        {
            _ = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_ADVANCE_FRAME, 0, IntPtr.Zero);
        }

        #endregion

        #region Other
        public void Dispose()
        {
            // this is called on non-emu thread because emu thread is stuck inside m64p
            // so dont do anything but sending close command
            if (!disposed)
            {
                while (isBusyInCore)
                {
                    // send command multiple times to assure it closes :/
                    _ = corePlugin.DoCommandPointer(EmulatorCommand.M64CMD_STOP, 0, IntPtr.Zero);
                }
            }
        }
        #endregion
    }
}