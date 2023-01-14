using System.Runtime.InteropServices;

namespace M64RPFW.Models.Emulation.API;

public static class Mupen64PlusTypes
{
    public enum ConsoleMemorySections : uint
    {
        Rdram = 1,
        PiReg,
        SiReg,
        ViReg,
        RiReg,
        AiReg,

        EepRom = 100,
        Mempak1,
        Mempak2,
        Mempak3,
        Mempak4,

        TheRom
    }

    public enum CoreTypes
    {
        PureInterpreter,
        CachedInterpreter,
        DynamicRecompiler
    }

    public enum DisplayTypes
    {
        Windowed,
        ExclusiveFullscreen
    }

    public enum EmulatorCommand
    {
        M64CmdNop = 0,
        M64CmdRomOpen,
        M64CmdRomClose,
        M64CmdRomGetHeader,
        M64CmdRomGetSettings,
        M64CmdExecute,
        M64CmdStop,
        M64CmdPause,
        M64CmdResume,
        M64CmdCoreStateQuery,
        M64CmdStateLoad,
        M64CmdStateSave,
        M64CmdStateSetSlot,
        M64CmdSendSdlKeydown,
        M64CmdSendSdlKeyup,
        M64CmdSetFrameCallback,
        M64CmdTakeNextScreenshot,
        M64CmdCoreStateSet,
        M64CmdReadScreen,
        M64CmdReset,
        M64CmdAdvanceFrame,
        M64CmdSetViCallback,
        M64CmdSetRenderCallback
    }

    public enum EmulatorCoreParameters
    {
        M64CoreEmuState = 1,
        M64CoreVideoMode,
        M64CoreSavestateSlot,
        M64CoreSpeedFactor,
        M64CoreSpeedLimiter,
        M64CoreVideoSize,
        M64CoreAudioVolume,
        M64CoreAudioMute,
        M64CoreInputGameshark,
        M64CoreStateLoadcomplete,
        M64CoreStateSavecomplete
    }

    public enum EmulatorGlAttributes
    {
        M64PGlDoublebuffer = 1,
        M64PGlBufferSize,
        M64PGlDepthSize,
        M64PGlRedSize,
        M64PGlGreenSize,
        M64PGlBlueSize,
        M64PGlAlphaSize,
        M64PGlSwapControl,
        M64PGlMultisamplebuffers,
        M64PGlMultisamplesamples,
        M64PGlContextMajorVersion,
        M64PGlContextMinorVersion,
        M64PGlContextProfileMask
    }

    public enum EmulatorPluginType
    {
        M64PluginNull = 0,
        M64PluginRsp = 1,
        M64PluginGfx,
        M64PluginAudio,
        M64PluginInput,
        M64PluginCore
    }


    public enum EmulatorStates
    {
        M64EmuStopped = 1,
        M64EmuRunning,
        M64EmuPaused
    }

    public enum EmulatorStatus
    {
        M64ErrSuccess = 0,
        M64ErrNotInit, /* Function is disallowed before InitMupen64Plus() is called */
        M64ErrAlreadyInit, /* InitMupen64Plus() was called twice */
        M64ErrIncompatible, /* API versions between components are incompatible */
        M64ErrInputAssert, /* Invalid parameters for function call, such as ParamValue=NULL for GetCoreParameter() */
        M64ErrInputInvalid, /* Invalid input data, such as ParamValue="maybe" for SetCoreParameter() to set a BOOL-type value */
        M64ErrInputNotFound, /* The input parameter(s) specified a particular item which was not found */
        M64ErrNoMemory, /* Memory allocation failed */
        M64ErrFiles, /* Error opening, creating, reading, or writing to a file */
        M64ErrPublic, /* public error (bug) */
        M64ErrInvalidState, /* Current program state does not allow operation */
        M64ErrPluginFail, /* A plugin function returned a fatal error */
        M64ErrSystemFail, /* A system function call, such as an SDL or file operation, failed */
        M64ErrUnsupported, /* Function call is not supported (ie, core not built with debugger) */
        M64ErrWrongType /* A given input type parameter cannot be used for desired operation */
    }

    public enum EmulatorSystemTypes
    {
        SystemNtsc = 0,
        SystemPal,
        SystemMpal
    }

    public enum EmulatorTypes
    {
        M64TypeInt = 1,
        M64TypeFloat,
        M64TypeBool,
        M64TypeString
    }

    public enum EmulatorVideoFlags
    {
        M64VideoflagSupportResizing = 1
    }

    public enum EmulatorVideoModes
    {
        M64VideoNone = 1,
        M64VideoWindowed,
        M64VideoFullscreen
    }

    public enum PlayModes
    {
        Stopped = 1,
        Running = 2,
        Paused = 3
    }

    public enum SaveStateTypes
    {
        Mupen64Plus,
        Project64Compressed,
        Project64Uncompressed
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EmulatorRomHeader
    {
        public byte init_PI_BSB_DOM1_LAT_REG; /* 0x00 */
        public byte init_PI_BSB_DOM1_PGS_REG; /* 0x01 */
        public byte init_PI_BSB_DOM1_PWD_REG; /* 0x02 */
        public byte init_PI_BSB_DOM1_PGS_REG2; /* 0x03 */
        public uint ClockRate; /* 0x04 */
        public uint PC; /* 0x08 */
        public uint Release; /* 0x0C */
        public uint CRC1; /* 0x10 */
        public uint CRC2; /* 0x14 */

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] Unknown; /* 0x18 */

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] Name; /* 0x20 */

        public uint unknown; /* 0x34 */
        public uint Manufacturer_ID; /* 0x38 */
        public ushort Cartridge_ID; /* 0x3C - Game serial number  */
        public ushort Country_code; /* 0x3E */
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EmulatorRomSettings
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public char[] goodname;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
        public char[] MD5;

        public byte savetype;
        public byte status; // Rom status on a scale fRom 0-5. 
        public byte players; // Local players 0-4, 2/3/4 way Netplay indicated by 5/6/7. 
        public byte rumble; // 0 - No, 1 - Yes boolean for rumble support. 
        public byte transferpak; // 0 - No, 1 - Yes boolean for transfer pak support. 
        public byte mempak; // 0 - No, 1 - Yes boolean for memory pak support. 
        public byte biopak; // 0 - No, 1 - Yes boolean for bio pak support. 
    }


    public struct EmulatorSize
    {
        public uint UiWidth;
        public uint UiHeight;
    }

    public unsafe struct EmulatorVideoExtensionFunctions
    {
        public uint Functions;
        public delegate* unmanaged[Cdecl]<EmulatorStatus> VidExtFuncInit;
        public delegate* unmanaged[Cdecl]<EmulatorStatus> VidExtFuncQuit;
        public delegate* unmanaged[Cdecl]<EmulatorSize*, int*, EmulatorStatus> VidExtFuncListModes;
        public delegate* unmanaged[Cdecl]<EmulatorSize, int*, int*, EmulatorStatus> VidExtFuncListRates;
        public delegate* unmanaged[Cdecl]<int, int, int, int, int, EmulatorStatus> VidExtFuncSetMode;
        public delegate* unmanaged[Cdecl]<int, int, int, int, int, int, EmulatorStatus> VidExtFuncSetModeWithRate;
        public delegate* unmanaged[Cdecl]<char*, delegate* unmanaged<void>> VidExtFuncGlGetProc;
        public delegate* unmanaged[Cdecl]<EmulatorGlAttributes, int, EmulatorStatus> VidExtFuncGlSetAttr;
        public delegate* unmanaged[Cdecl]<EmulatorGlAttributes, int*, EmulatorStatus> VidExtFuncGlGetAttr;
        public delegate* unmanaged[Cdecl]<EmulatorStatus> VidExtFuncGlSwapBuf;
        public delegate* unmanaged[Cdecl]<char*, EmulatorStatus> VidExtFuncSetCaption;
        public delegate* unmanaged[Cdecl]<EmulatorStatus> VidExtFuncToggleFs;
        public delegate* unmanaged[Cdecl]<int, int, EmulatorStatus> VidExtFuncResizeWindow;
        public delegate* unmanaged[Cdecl]<uint> VidExtFuncGlGetDefaultFramebuffer;
    }
}