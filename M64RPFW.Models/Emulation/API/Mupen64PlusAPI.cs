using M64RPFW.Models.Emulation.Exceptions;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.API
{
    public sealed class Mupen64PlusAPI : IDisposable
    {
        #region P/Invoke

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        #endregion

        #region Delegates

        /// <summary>
        /// Initializes the the core DLL
        /// </summary>
        /// <param name="APIVersion">Specifies what API version our app is using. Just set this to 0x20001</param>
        /// <param name="ConfigPath">Directory to have the DLL look for config data. "" seems to disable this</param>
        /// <param name="DataPath">Directory to have the DLL look for user data. "" seems to disable this</param>
        /// <param name="Context">Use "Core"</param>
        /// <param name="DebugCallback">A function to use when the core wants to output debug messages</param>
        /// <param name="context2">Use ""</param>
        /// <param name="dummy">Use IntPtr.Zero</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreStartup(int APIVersion, string ConfigPath, string DataPath, string Context, DebugCallback DebugCallback, string context2, IntPtr dummy);

        private CoreStartup? m64pCoreStartup;

        /// <summary>
        /// Cleans up the core
        /// </summary>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreShutdown();

        private CoreShutdown? m64pCoreShutdown;

        /// <summary>
        /// Connects a plugin DLL to the core DLL
        /// </summary>
        /// <param name="PluginType">The type of plugin that is being connected</param>
        /// <param name="PluginLibHandle">The DLL handle for the plugin</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreAttachPlugin(m64p_plugin_type PluginType, IntPtr PluginLibHandle);

        private CoreAttachPlugin? m64pCoreAttachPlugin;

        /// <summary>
        /// Disconnects a plugin DLL from the core DLL
        /// </summary>
        /// <param name="PluginType">The type of plugin to be disconnected</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDetachPlugin(m64p_plugin_type PluginType);

        private CoreDetachPlugin? m64pCoreDetachPlugin;

        /// <summary>
        /// Opens a section in the global config system
        /// </summary>
        /// <param name="SectionName">The name of the section to open</param>
        /// <param name="ConfigSectionHandle">A pointer to the pointer to use as the section handle</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error ConfigOpenSection(string SectionName, ref IntPtr ConfigSectionHandle);

        private ConfigOpenSection? m64pConfigOpenSection;

        /// <summary>
        /// Sets an Int parameter in the global config system
        /// </summary>
        /// <param name="ConfigSectionHandle">The handle of the section to access</param>
        /// <param name="ParamName">The name of the parameter to set</param>
        /// <param name="ParamType">The type of the parameter</param>
        /// <param name="ParamValue">A pointer to the value to use for the parameter (must be an int right now)</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error ConfigSetParameterInt(IntPtr ConfigSectionHandle, string ParamName, m64p_type ParamType, ref int ParamValue);

        private ConfigSetParameterInt? m64pConfigSetParameterInt;

        /// <summary>
		/// Sets a parameter in the global config system
		/// </summary>
		/// <param name="ConfigSectionHandle">The handle of the section to access</param>
		/// <param name="ParamName">The name of the parameter to set</param>
		/// <param name="ParamType">The type of the parameter</param>
		/// <param name="ParamValue">A pointer to the value to use for the parameter (must be a string)</param>
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error ConfigSetParameterStr(IntPtr ConfigSectionHandle, string ParamName, m64p_type ParamType, StringBuilder ParamValue);

        private ConfigSetParameterStr? m64pConfigSetParameterStr;

        /// <summary>
        /// Sets a String parameter in the global config system
        /// </summary>
        /// <param name="ConfigSectionHandle">The handle of the section to access</param>
        /// <param name="ParamName">The name of the parameter to set</param>
        /// <param name="ParamType">The type of the parameter</param>
        /// <param name="ParamValue">A pointer to the value to use for the parameter (must be an int right now)</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error ConfigSetParameterBool(IntPtr ConfigSectionHandle, string ParamName, m64p_type ParamType, ref bool ParamValue);

        private ConfigSetParameterBool? m64pConfigSetParameterBool;

        /// <summary>
        /// Sets a parameter in the global config system
        /// </summary>
        /// <param name="ConfigSectionHandle">The handle of the section to access</param>
        /// <param name="ParamName">The name of the parameter to set</param>
        /// <param name="ParamType">The type of the parameter</param>
        /// <param name="ParamValue">A pointer to the value to use for the parameter (must be an int right now)</param>
        /// <returns></returns>
        /*dont know if this even works*/
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error ConfigSetPlugins(IntPtr ConfigSectionHandle, string ParamName, m64p_type ParamType, ref string ParamValue);

        private ConfigSetPlugins? m64pConfigSetPlugins;

        /// <summary>
        /// Saves the mupen64plus state to the provided buffer
        /// </summary>
        /// <param name="buffer">A byte array to use to save the state. Must be at least 16788288 + 1024 bytes</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int savestates_save_bkm(byte[] buffer);

        private readonly savestates_save_bkm? m64pCoreSaveState;

        /// <summary>
        /// Loads the mupen64plus state from the provided buffer
        /// </summary>
        /// <param name="buffer">A byte array filled with the state to load. Must be at least 16788288 + 1024 bytes</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int savestates_load_bkm(byte[] buffer);

        private readonly savestates_load_bkm? m64pCoreLoadState;

        /// <summary>
        /// Gets a pointer to a section of the mupen64plus core
        /// </summary>
        /// <param name="mem_ptr_type">The section to get a pointer for</param>
        /// <returns>A pointer to the section requested</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr DebugMemGetPointer(N64_MEMORY mem_ptr_type);

        private DebugMemGetPointer? m64pDebugMemGetPointer;

        /// <summary>
        /// Gets the size of the given memory area
        /// </summary>
        /// <param name="mem_ptr_type">The section to get the size of</param>
        /// <returns>The size of the section requested</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MemGetSize(N64_MEMORY mem_ptr_type);

        private readonly MemGetSize? m64pMemGetSize;

        /// <summary>
        /// Initializes the saveram (eeprom and 4 mempacks)
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr init_saveram();

        private readonly init_saveram? m64pinit_saveram;

        /// <summary>
        /// Pulls out the saveram for bizhawk to save
        /// </summary>
        /// <param name="dest">A byte array to save the saveram into</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr save_saveram(byte[] dest);

        private readonly save_saveram? m64psave_saveram;

        /// <summary>
        /// Restores the saveram from bizhawk
        /// </summary>
        /// <param name="src">A byte array containing the saveram to restore</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr load_saveram(byte[] src);

        private readonly load_saveram? m64pload_saveram;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandByteArray(m64p_command Command, int ParamInt, byte[] ParamPtr);

        private CoreDoCommandByteArray? m64pCoreDoCommandByteArray;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandPtr(m64p_command Command, int ParamInt, IntPtr ParamPtr);

        private CoreDoCommandPtr? m64pCoreDoCommandPtr;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandRefPtr(m64p_command Command, ref int ParamInt, IntPtr ParamPtr);

        private CoreDoCommandRefPtr? m64pCoreDoCommandRefPtr;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandStr(m64p_command Command, int ParamInt, string ParamPtr);

        private CoreDoCommandStr? m64pCoreDoCommandStr;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandROMHeader(m64p_command Command, int ParamInt, ref m64p_rom_header ParamPtr);

        private CoreDoCommandROMHeader? m64pCoreDoCommandROMHeader;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandROMSettings(m64p_command Command, m64p_rom_settings ParamInt, ref int ParamPtr);

        private CoreDoCommandROMSettings? m64pCoreDoCommandROMSettings;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandCoreStateQuery(m64p_command Command, m64p_core_param ParamInt, int ParamPtr);

        private CoreDoCommandCoreStateQuery? m64pCoreDoCommandCoreStateQuery;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandCoreStateSet(m64p_command Command, m64p_core_param ParamInt, ref int ParamPtr);

        private CoreDoCommandCoreStateSet? m64pCoreDoCommandCoreStateSet;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandCoreStateSetVideoMode(m64p_command Command, m64p_video_mode ParamInt, IntPtr ParamPtr/* ref int ParamPtr*/);

        private CoreDoCommandCoreStateSetVideoMode? m64pCoreDoCommandCoreStateVideoMode;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandCoreStateSetRef(m64p_command Command, m64p_core_param ParamInt, ref int ParamPtr);

        private CoreDoCommandCoreStateSetRef? m64pCoreDoCommandCoreStateSetRef;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandRefInt(m64p_command Command, int ParamInt, ref int ParamPtr);

        private CoreDoCommandRefInt? m64pCoreDoCommandRefInt;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandFrameCallback(m64p_command Command, int ParamInt, FrameCallback ParamPtr);

        private CoreDoCommandFrameCallback? m64pCoreDoCommandFrameCallback;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandVICallback(m64p_command Command, int ParamInt, VICallback ParamPtr);

        private CoreDoCommandVICallback? m64pCoreDoCommandVICallback;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreDoCommandRenderCallback(m64p_command Command, int ParamInt, RenderCallback ParamPtr);

        private CoreDoCommandRenderCallback? m64pCoreDoCommandRenderCallback;

        // Configuration
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error ConfigSetDefaultFloat(string ConfigSectionHandle, string ParamName, double ParamValue, string ParamHelp);

        private ConfigSetDefaultFloat? m64pConfigSetDefaultFloat;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error ConfigSetDefaultString(string ConfigSectionHandle, string ParamName, string ParamValue, string ParamHelp);

        private ConfigSetDefaultString? m64pConfigSetDefaultString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error ConfigSaveFile();

        private ConfigSaveFile? m64pConfigSaveFile;

        //WARNING - RETURNS A STATIC BUFFER
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr biz_r4300_decode_op(uint instr, uint counter);
        public biz_r4300_decode_op? m64p_decode_op;

        /// <summary>
        /// Reads from the "system bus"
        /// </summary>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte biz_read_memory(uint addr);
        public biz_read_memory? m64p_read_memory_8;

        /// <summary>
        /// Writes to the "system bus"
        /// </summary>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void biz_write_memory(uint addr, byte value);
        public biz_write_memory? m64p_write_memory_8;

        // These are common for all four plugins

        /// <summary>
        /// Initializes the plugin
        /// </summary>
        /// <param name="CoreHandle">The DLL handle for the core DLL</param>
        /// <param name="Context">Giving a context to the DebugCallback</param>
        /// <param name="DebugCallback">A function to use when the pluging wants to output debug messages</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate m64p_error PluginStartup(IntPtr CoreHandle, string Context, DebugCallback DebugCallback);

        /// <summary>
        /// Cleans up the plugin
        /// </summary>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate m64p_error PluginShutdown();

        // Callback functions

        /// <summary>
        /// Handles a debug message from mupen64plus
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="level"></param>
        /// <param name="Message">The message to display</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DebugCallback(IntPtr Context, int level, string Message);

        /// <summary>
        /// This will be called every time a new frame is finished
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FrameCallback();

        private FrameCallback? m64pFrameCallback;

        /// <summary>
        /// This will be called every time a VI occurs
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VICallback();

        private VICallback? m64pVICallback;

        /// <summary>
        /// This will be called every time before the screen is drawn
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void RenderCallback();

        private RenderCallback? m64pRenderCallback;

        /// <summary>
        /// This will be called after the emulator is setup and is ready to be used
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StartupCallback();

        /// <summary>
        /// Type of the read/write memory callbacks
        /// </summary>
        /// <param name="address">The address which the cpu is read/writing</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MemoryCallback(uint address);

        /// <summary>
        /// Sets the memory read callback
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetReadCallback(MemoryCallback callback);

        private readonly SetReadCallback? m64pSetReadCallback;

        /// <summary>
        /// Sets the memory write callback
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetWriteCallback(MemoryCallback callback);

        private readonly SetWriteCallback? m64pSetWriteCallback;

        /// <summary>
        /// Gets the CPU registers
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetRegisters(byte[] dest);

        private readonly GetRegisters? m64pGetRegisters;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate m64p_error CoreAddCheat(string CheatName, string[] CodeList, int NumCodes);

        private CoreAddCheat? m64pCoreAddCheat;

        /// <summary>
        /// Fills a provided buffer with the mupen64plus framebuffer
        /// </summary>
        /// <param name="framebuffer">The buffer to fill</param>
        /// <param name="width">A pointer to a variable to fill with the width of the framebuffer</param>
        /// <param name="height">A pointer to a variable to fill with the height of the framebuffer</param>
        /// <param name="buffer">Which buffer to read: 0 = front, 1 = back</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ReadScreen2(int[] framebuffer, ref int width, ref int height, int buffer);

        private ReadScreen2? GFXReadScreen2;

        /// <summary>
        /// Gets the width and height of the mupen64plus framebuffer
        /// </summary>
        /// <param name="dummy">Use IntPtr.Zero</param>
        /// <param name="width">A pointer to a variable to fill with the width of the framebuffer</param>
        /// <param name="height">A pointer to a variable to fill with the height of the framebuffer</param>
        /// <param name="buffer">Which buffer to read: 0 = front, 1 = back</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ReadScreen2Res(IntPtr dummy, ref int width, ref int height, int buffer);

        private ReadScreen2Res? GFXReadScreen2Res;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetScreenTextureID();

        private GetScreenTextureID? GFXGetScreenTextureID;

        #endregion

        #region Events

        public event Action OnFrameFinished;
        public event Action OnVIInterrupt;
        public event Action OnRender;

        public event Action OnFrameBufferCreated;
        public event Action OnFrameBufferUpdate;

        #endregion

        #region Private Fields

        private bool disposed = false;
        private readonly ManualResetEvent m64pStartupComplete = new(false);
        private readonly Task? m64pEmulator;
        private volatile bool isBusyInCore;
        private IntPtr coreDll;

        #endregion

        #region Properties
        public bool IsEmulatorRunning => isBusyInCore;
        public bool IsFrameBufferInitialized => FrameBuffer != null && BufferWidth != 0 && BufferHeight != 0;
        public int[]? FrameBuffer { get; private set; }
        public int BufferWidth { get; private set; }
        public int BufferHeight { get; private set; }

        #endregion

        private void GetScreenDimensions(out int width, out int height)
        {
            int w = 0, h = 0;
            GFXReadScreen2Res(IntPtr.Zero, ref w, ref h, 0);
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
                OnFrameBufferCreated?.Invoke();
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
                AllocateFrameBuffer(2, 2);
            }

            int[] frameBuffer = FrameBuffer;
            int bufferWidth = BufferWidth, bufferHeight = BufferHeight;
            GFXReadScreen2(frameBuffer, ref bufferWidth, ref bufferHeight, 0);
            FrameBuffer = frameBuffer;
            BufferWidth = bufferWidth;
            BufferHeight = bufferHeight;
        }

        private void SetSetting(string section, string name, object value, bool save = false)
        {
            Debug.Print($"{section} {name} - {value}");

            IntPtr _section = IntPtr.Zero;
            _ = m64pConfigOpenSection(section, ref _section);

            _ = value is int intValue
                ? m64pConfigSetParameterInt(_section, name, m64p_type.M64TYPE_INT, ref intValue)
                : value is bool boolValue
                    ? m64pConfigSetParameterBool(_section, name, m64p_type.M64TYPE_BOOL, ref boolValue)
                    : value is string stringValue
                                    ? m64pConfigSetParameterStr(_section, name, m64p_type.M64TYPE_STRING, new StringBuilder(stringValue))
                                    : throw new UnresolvableConfigEntryTypeException($"Type {value.GetType()} could not be resolved to a type accepted by m64p+");

            if (save)
            {
                _ = m64pConfigSaveFile();
            }
        }

        private void ApplyConfig(Mupen64PlusConfig config)
        {
            foreach (FieldInfo item in config.GetType().GetFields())
            {
                Mupen64PlusConfigEntryAttribute? attrib = item.GetCustomAttribute<Mupen64PlusConfigEntryAttribute>();
                SetSetting(attrib.Section, attrib.Name, item.GetValue(config));
            }

            _ = m64pConfigSaveFile();
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

            coreDll = NativeLibrary.Load(launchParameters.CoreLibraryPath);

            ConnectFunctionPointers();

            m64p_error result;

            result = m64pCoreStartup(
                0x20001,
                "Config/",
                "",
                "Core",
                null,
                "",
                IntPtr.Zero
            );

            ApplyConfig(launchParameters.Config);

            int enc = (launchParameters.Config.ScreenWidth << 16) + launchParameters.Config.ScreenHeight;
            result = m64pCoreDoCommandCoreStateSet(
                    m64p_command.M64CMD_CORE_STATE_SET,
                    m64p_core_param.M64CORE_VIDEO_SIZE,
                    ref enc
                );

            result = m64pCoreDoCommandPtr(m64p_command.M64CMD_STATE_SET_SLOT, launchParameters.InitialSlot, IntPtr.Zero);

            result = m64pCoreDoCommandByteArray(m64p_command.M64CMD_ROM_OPEN, launchParameters.Rom.Length, launchParameters.Rom);
            int sizeHeader = Marshal.SizeOf(typeof(m64p_rom_header));

            _ = AttachPlugin(m64p_plugin_type.M64PLUGIN_GFX, launchParameters.VideoPluginPath);
            _ = AttachPlugin(m64p_plugin_type.M64PLUGIN_AUDIO, launchParameters.AudioPluginPath);
            _ = AttachPlugin(m64p_plugin_type.M64PLUGIN_INPUT, launchParameters.InputPluginPath);
            _ = AttachPlugin(m64p_plugin_type.M64PLUGIN_RSP, launchParameters.RSPPluginPath);

            GFXReadScreen2 = GetTypedDelegate<ReadScreen2>(plugins[m64p_plugin_type.M64PLUGIN_GFX].Handle, "ReadScreen2");
            GFXReadScreen2Res = GetTypedDelegate<ReadScreen2Res>(plugins[m64p_plugin_type.M64PLUGIN_GFX].Handle, "ReadScreen2");
            IntPtr funcPtr = GetProcAddress(plugins[m64p_plugin_type.M64PLUGIN_GFX].Handle, "GetScreenTextureID");
            if (funcPtr != IntPtr.Zero)
            {
                GFXGetScreenTextureID = (GetScreenTextureID)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(GetScreenTextureID));
            }

            m64pFrameCallback = new FrameCallback(delegate
            {
                UpdateFramebuffer();
                OnFrameFinished?.Invoke();
                if (IsFrameBufferInitialized)
                {
                    OnFrameBufferUpdate?.Invoke();
                }
            });

            result = m64pCoreDoCommandFrameCallback(m64p_command.M64CMD_SET_FRAME_CALLBACK, 0, m64pFrameCallback);

            m64pVICallback = new VICallback(delegate
            {
                OnVIInterrupt?.Invoke();
            });
            result = m64pCoreDoCommandVICallback(m64p_command.M64CMD_SET_VI_CALLBACK, 0, m64pVICallback);

            m64pRenderCallback = new RenderCallback(delegate
            {
                OnRender?.Invoke();
            });
            result = m64pCoreDoCommandRenderCallback(m64p_command.M64CMD_SET_RENDER_CALLBACK, 0, m64pRenderCallback);

            ExecuteEmulator();
        }

        /// <summary>
        /// Starts execution of mupen64plus
        /// Does not return until the emulator stops
        /// </summary>
        private void ExecuteEmulator()
        {
            isBusyInCore = true;

            StartupCallback cb = new(() => m64pStartupComplete.Set());
            _ = m64pCoreDoCommandPtr(m64p_command.M64CMD_EXECUTE, 0, Marshal.GetFunctionPointerForDelegate(cb));

            isBusyInCore = false;

            Debug.Print("Escaped core code");

            // stopped 
            DetachPlugin(m64p_plugin_type.M64PLUGIN_GFX);
            DetachPlugin(m64p_plugin_type.M64PLUGIN_AUDIO);
            DetachPlugin(m64p_plugin_type.M64PLUGIN_INPUT);
            DetachPlugin(m64p_plugin_type.M64PLUGIN_RSP);

            _ = m64pCoreDoCommandPtr(m64p_command.M64CMD_ROM_CLOSE, 0, IntPtr.Zero);

            _ = m64pCoreShutdown();

            NativeLibrary.Free(coreDll);

            // TODO:
            // BUG:
            // the auto-created Rice video window does something weird and causes any subsequent child window to freeze up main message pump
        }

        #region Interaction Functions

        public void SetStateSlot(int slot)
        {
            _ = m64pCoreDoCommandPtr(m64p_command.M64CMD_STATE_SET_SLOT, slot, IntPtr.Zero);
        }

        public m64p_rom_header GetROMHeader(m64p_rom_header _rom_header)
        {
            int size = Marshal.SizeOf(typeof(m64p_rom_header));

            _ = m64pCoreDoCommandROMHeader(m64p_command.M64CMD_ROM_GET_HEADER, size, ref _rom_header);

            return _rom_header;
        }

        public m64p_rom_settings GetROMSettings(m64p_rom_settings _rom_settings)
        {
            int size = Marshal.SizeOf(typeof(m64p_rom_settings));

            _ = m64pCoreDoCommandROMSettings(m64p_command.M64CMD_ROM_GET_SETTINGS, _rom_settings, ref size);

            return _rom_settings;
        }

        public void LoadState(int slot)
        {
            _ = m64pCoreDoCommandPtr(m64p_command.M64CMD_STATE_LOAD, slot, IntPtr.Zero);
        }

        public void SaveState(int slot)
        {
            _ = m64pCoreDoCommandPtr(m64p_command.M64CMD_STATE_SAVE, slot, IntPtr.Zero);
        }
        public void LoadState(string filePath)
        {
            _ = m64pCoreDoCommandStr(m64p_command.M64CMD_STATE_LOAD, 0, filePath);
        }

        public void SaveState(string filePath, SaveStateTypes type = SaveStateTypes.Mupen64Plus)
        {
            _ = m64pCoreDoCommandStr(m64p_command.M64CMD_STATE_SAVE, (int)type, filePath);
        }

        public void Screenshot()
        {
            _ = m64pCoreDoCommandPtr(m64p_command.M64CMD_TAKE_NEXT_SCREENSHOT, 0, IntPtr.Zero);
        }

        public void SetPlayMode(PlayModes playMode)
        {
            int dummy = (int)playMode;
            _ = m64pCoreDoCommandCoreStateSet(
                    m64p_command.M64CMD_CORE_STATE_SET,
                    m64p_core_param.M64CORE_EMU_STATE,
                    ref dummy
                );
        }

        public void SetSoundMode(bool silence)
        {
            int dummy = silence ? 1 : 0;
            _ = m64pCoreDoCommandCoreStateSet(
                m64p_command.M64CMD_CORE_STATE_SET,
                m64p_core_param.M64CORE_AUDIO_MUTE,
                ref dummy
            );
        }

        public void Stop()
        {
            Dispose();
        }

        public void Reset(bool soft)
        {
            _ = m64pCoreDoCommandPtr(m64p_command.M64CMD_RESET, soft ? 0 : 1, IntPtr.Zero);
        }

        public void SetSpeedFactor(int speed)
        {
            _ = m64pCoreDoCommandCoreStateSet(
                m64p_command.M64CMD_CORE_STATE_SET,
                m64p_core_param.M64CORE_SPEED_FACTOR,
                ref speed
            );
        }

        public void SetVideoMode(m64p_video_mode mode)
        {
            int dummy = (int)mode;
            _ = m64pCoreDoCommandCoreStateSet(
                    m64p_command.M64CMD_CORE_STATE_SET,
                    m64p_core_param.M64CORE_VIDEO_MODE,
                    ref dummy);
        }

        public int GetMemorySize(N64_MEMORY id)
        {
            return m64pMemGetSize(id);
        }

        public IntPtr GetMemoryPointer(N64_MEMORY id)
        {
            return m64pDebugMemGetPointer(id);
        }


        public void FrameAdvance()
        {
            _ = m64pCoreDoCommandPtr(m64p_command.M64CMD_ADVANCE_FRAME, 0, IntPtr.Zero);
        }

        public int SaveState(byte[] buffer)
        {
            return m64pCoreSaveState(buffer);
        }

        public void LoadState(byte[] buffer)
        {
            _ = m64pCoreLoadState(buffer);
        }

        private readonly byte[]? saveram_backup;

        public void InitSaveram()
        {
            _ = m64pinit_saveram();
        }

        public const int kSaveramSize = 0x800 + (4 * 0x8000) + 0x20000 + 0x8000;

        public byte[] SaveSaveram()
        {
            if (disposed)
            {
                if (saveram_backup != null)
                {
                    return (byte[])saveram_backup.Clone();
                }
                else
                {
                    // This shouldn't happen!!
                    return new byte[kSaveramSize];
                }
            }
            else
            {
                byte[] dest = new byte[kSaveramSize];
                _ = m64psave_saveram(dest);
                return dest;
            }
        }

        public void LoadSaveram(byte[] src)
        {
            _ = m64pload_saveram(src);
        }

        public void SetReadCallbackFunc(MemoryCallback callback)
        {
            m64pSetReadCallback(callback);
        }

        public void SetWriteCallbackFunc(MemoryCallback callback)
        {
            m64pSetWriteCallback(callback);
        }

        public void GetRegistersFunc(byte[] dest)
        {
            m64pGetRegisters(dest);
        }

        #endregion

        // bizhawk
        internal static T GetTypedDelegate<T>(IntPtr lib, string proc) where T : Delegate
        {
            return (T)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(lib, proc), typeof(T));
        }

        /// <summary>
        /// Look up function pointers in the dlls
        /// </summary>
        private void ConnectFunctionPointers()
        {
            m64pCoreStartup = (CoreStartup)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreStartup"), typeof(CoreStartup));
            m64pCoreShutdown = (CoreShutdown)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreShutdown"), typeof(CoreShutdown));
            m64pCoreDoCommandByteArray = (CoreDoCommandByteArray)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandByteArray));
            m64pCoreDoCommandPtr = (CoreDoCommandPtr)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandPtr));
            // Custom
            m64pCoreDoCommandRefPtr = (CoreDoCommandRefPtr)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandRefPtr));
            m64pCoreDoCommandStr = (CoreDoCommandStr)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandStr));
            m64pCoreDoCommandROMHeader = (CoreDoCommandROMHeader)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandROMHeader));
            m64pCoreDoCommandROMSettings = (CoreDoCommandROMSettings)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandROMSettings));
            m64pCoreDoCommandCoreStateSet = (CoreDoCommandCoreStateSet)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandCoreStateSet));
            m64pCoreDoCommandCoreStateVideoMode = (CoreDoCommandCoreStateSetVideoMode)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandCoreStateSetVideoMode));
            m64pCoreDoCommandCoreStateSetRef = (CoreDoCommandCoreStateSetRef)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandCoreStateSetRef));
            m64pCoreDoCommandCoreStateQuery = (CoreDoCommandCoreStateQuery)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandCoreStateQuery));
            m64pConfigSetDefaultFloat = (ConfigSetDefaultFloat)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "ConfigSetDefaultFloat"), typeof(ConfigSetDefaultFloat));
            m64pConfigSetDefaultString = (ConfigSetDefaultString)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "ConfigSetDefaultString"), typeof(ConfigSetDefaultString));
            m64pConfigSaveFile = (ConfigSaveFile)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "ConfigSaveFile"), typeof(ConfigSaveFile));
            m64pCoreAddCheat = (CoreAddCheat)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreAddCheat"), typeof(CoreAddCheat));
            // End Custom
            m64pCoreDoCommandRefInt = (CoreDoCommandRefInt)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandRefInt));
            m64pCoreDoCommandFrameCallback = (CoreDoCommandFrameCallback)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandFrameCallback));
            m64pCoreDoCommandVICallback = (CoreDoCommandVICallback)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandVICallback));
            m64pCoreDoCommandRenderCallback = (CoreDoCommandRenderCallback)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDoCommand"), typeof(CoreDoCommandRenderCallback));
            m64pCoreAttachPlugin = (CoreAttachPlugin)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreAttachPlugin"), typeof(CoreAttachPlugin));
            m64pCoreDetachPlugin = (CoreDetachPlugin)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "CoreDetachPlugin"), typeof(CoreDetachPlugin));
            m64pConfigOpenSection = (ConfigOpenSection)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "ConfigOpenSection"), typeof(ConfigOpenSection));
            m64pConfigSetParameterStr = (ConfigSetParameterStr)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "ConfigSetParameter"), typeof(ConfigSetParameterStr));
            m64pConfigSetParameterInt = (ConfigSetParameterInt)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "ConfigSetParameter"), typeof(ConfigSetParameterInt));
            m64pConfigSetParameterBool = (ConfigSetParameterBool)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "ConfigSetParameter"), typeof(ConfigSetParameterBool));

            m64pConfigSetPlugins = (ConfigSetPlugins)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "ConfigSetParameter"), typeof(ConfigSetPlugins));
            m64pDebugMemGetPointer = (DebugMemGetPointer)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(coreDll, "DebugMemGetPointer"), typeof(DebugMemGetPointer));

        }





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
                    _ = m64pCoreDoCommandPtr(m64p_command.M64CMD_STOP, 0, IntPtr.Zero);
                }
            }
        }

        internal struct AttachedPlugin
        {
            internal PluginStartup StartupDelegate;
            internal PluginShutdown ShutdownDelegate;
            internal IntPtr Handle;
        }

        private readonly Dictionary<m64p_plugin_type, AttachedPlugin> plugins = new();

        private IntPtr AttachPlugin(m64p_plugin_type type, string libraryPath)
        {
            if (plugins.ContainsKey(type))
            {
                throw new PluginAlreadyAttachedException($"Plugin of type {type} is already attached");
            }

            AttachedPlugin plugin;
            plugin.Handle = NativeLibrary.Load(libraryPath);

            plugin.StartupDelegate = (PluginStartup)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(plugin.Handle, "PluginStartup"), typeof(PluginStartup));
            plugin.ShutdownDelegate = (PluginShutdown)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(plugin.Handle, "PluginShutdown"), typeof(PluginShutdown));
            _ = plugin.StartupDelegate(coreDll, null, null);

            if (m64pCoreAttachPlugin(type, plugin.Handle) != m64p_error.M64ERR_SUCCESS)
            {
                NativeLibrary.Free(plugin.Handle);
                throw new PluginAttachException($"Plugin of type {type} failed to attach");
            }

            plugins.Add(type, plugin);
            return plugin.Handle;
        }

        private void DetachPlugin(m64p_plugin_type type)
        {
            if (plugins.ContainsKey(type))
            {
                AttachedPlugin plugin = plugins[type];
                _ = m64pCoreDetachPlugin(type);
                _ = plugin.ShutdownDelegate();
                NativeLibrary.Free(plugin.Handle);
                _ = plugins.Remove(type);
            }
            else
            {
                throw new PluginDetachedException($"Plugin of type {type} is not attached");
            }
        }

        #endregion
    }
}