﻿using System.Runtime.InteropServices;

namespace M64RPFW.Models.Emulation.API
{
    public static class Mupen64PlusTypes
    {
        public enum EmulatorStatus
        {
            M64ERR_SUCCESS = 0,
            M64ERR_NOT_INIT,        /* Function is disallowed before InitMupen64Plus() is called */
            M64ERR_ALREADY_INIT,    /* InitMupen64Plus() was called twice */
            M64ERR_INCOMPATIBLE,    /* API versions between components are incompatible */
            M64ERR_INPUT_ASSERT,    /* Invalid parameters for function call, such as ParamValue=NULL for GetCoreParameter() */
            M64ERR_INPUT_INVALID,   /* Invalid input data, such as ParamValue="maybe" for SetCoreParameter() to set a BOOL-type value */
            M64ERR_INPUT_NOT_FOUND, /* The input parameter(s) specified a particular item which was not found */
            M64ERR_NO_MEMORY,       /* Memory allocation failed */
            M64ERR_FILES,           /* Error opening, creating, reading, or writing to a file */
            M64ERR_public,        /* public error (bug) */
            M64ERR_INVALID_STATE,   /* Current program state does not allow operation */
            M64ERR_PLUGIN_FAIL,     /* A plugin function returned a fatal error */
            M64ERR_SYSTEM_FAIL,     /* A system function call, such as an SDL or file operation, failed */
            M64ERR_UNSUPPORTED,     /* Function call is not supported (ie, core not built with debugger) */
            M64ERR_WRONG_TYPE       /* A given input type parameter cannot be used for desired operation */
        };

        public enum EmulatorPluginType
        {
            M64PLUGIN_NULL = 0,
            M64PLUGIN_RSP = 1,
            M64PLUGIN_GFX,
            M64PLUGIN_AUDIO,
            M64PLUGIN_INPUT,
            M64PLUGIN_CORE
        };

        public enum EmulatorCommand
        {
            M64CMD_NOP = 0,
            M64CMD_ROM_OPEN,
            M64CMD_ROM_CLOSE,
            M64CMD_ROM_GET_HEADER,
            M64CMD_ROM_GET_SETTINGS,
            M64CMD_EXECUTE,
            M64CMD_STOP,
            M64CMD_PAUSE,
            M64CMD_RESUME,
            M64CMD_CORE_STATE_QUERY,
            M64CMD_STATE_LOAD,
            M64CMD_STATE_SAVE,
            M64CMD_STATE_SET_SLOT,
            M64CMD_SEND_SDL_KEYDOWN,
            M64CMD_SEND_SDL_KEYUP,
            M64CMD_SET_FRAME_CALLBACK,
            M64CMD_TAKE_NEXT_SCREENSHOT,
            M64CMD_CORE_STATE_SET,
            M64CMD_READ_SCREEN,
            M64CMD_RESET,
            M64CMD_ADVANCE_FRAME,
            M64CMD_SET_VI_CALLBACK,
            M64CMD_SET_RENDER_CALLBACK
        };

        public enum EmulatorCoreParameters
        {
            M64CORE_EMU_STATE = 1,
            M64CORE_VIDEO_MODE,
            M64CORE_SAVESTATE_SLOT,
            M64CORE_SPEED_FACTOR,
            M64CORE_SPEED_LIMITER,
            M64CORE_VIDEO_SIZE,
            M64CORE_AUDIO_VOLUME,
            M64CORE_AUDIO_MUTE,
            M64CORE_INPUT_GAMESHARK,
            M64CORE_STATE_LOADCOMPLETE,
            M64CORE_STATE_SAVECOMPLETE
        };

        public enum EmulatorVideoModes
        {
            M64VIDEO_NONE = 1,
            M64VIDEO_WINDOWED,
            M64VIDEO_FULLSCREEN
        };


        public enum EmulatorStates
        {
            M64EMU_STOPPED = 1,
            M64EMU_RUNNING,
            M64EMU_PAUSED
        };

        public enum EmulatorTypes
        {
            M64TYPE_INT = 1,
            M64TYPE_FLOAT,
            M64TYPE_BOOL,
            M64TYPE_STRING
        };

        public enum ConsoleMemorySections : uint
        {
            RDRAM = 1,
            PI_REG,
            SI_REG,
            VI_REG,
            RI_REG,
            AI_REG,

            EEPRom = 100,
            MEMPAK1,
            MEMPAK2,
            MEMPAK3,
            MEMPAK4,

            THE_Rom
        }

        public enum EmulatorSystemTypes
        {
            SYSTEM_NTSC = 0,
            SYSTEM_PAL,
            SYSTEM_MPAL
        };

        public enum EmulatorVideoFlags
        {
            M64VIDEOFLAG_SUPPORT_RESIZING = 1
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EmulatorRomHeader
        {
            public byte init_PI_BSB_DOM1_LAT_REG;  /* 0x00 */
            public byte init_PI_BSB_DOM1_PGS_REG;  /* 0x01 */
            public byte init_PI_BSB_DOM1_PWD_REG;  /* 0x02 */
            public byte init_PI_BSB_DOM1_PGS_REG2; /* 0x03 */
            public uint ClockRate;                 /* 0x04 */
            public uint PC;                        /* 0x08 */
            public uint Release;                   /* 0x0C */
            public uint CRC1;                      /* 0x10 */
            public uint CRC2;                      /* 0x14 */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] Unknown;                 /* 0x18 */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Name;                    /* 0x20 */
            public uint unknown;                   /* 0x34 */
            public uint Manufacturer_ID;           /* 0x38 */
            public ushort Cartridge_ID;            /* 0x3C - Game serial number  */
            public ushort Country_code;            /* 0x3E */
        };


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EmulatorRomSettings
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] goodname;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
            public char[] MD5;
            public byte savetype;
            public byte status;         // Rom status on a scale fRom 0-5. 
            public byte players;        // Local players 0-4, 2/3/4 way Netplay indicated by 5/6/7. 
            public byte rumble;         // 0 - No, 1 - Yes boolean for rumble support. 
            public byte transferpak;    // 0 - No, 1 - Yes boolean for transfer pak support. 
            public byte mempak;         // 0 - No, 1 - Yes boolean for memory pak support. 
            public byte biopak;         // 0 - No, 1 - Yes boolean for bio pak support. 
        }

        public enum EmulatorGLAttributes
        {
            M64P_GL_DOUBLEBUFFER = 1,
            M64P_GL_BUFFER_SIZE,
            M64P_GL_DEPTH_SIZE,
            M64P_GL_RED_SIZE,
            M64P_GL_GREEN_SIZE,
            M64P_GL_BLUE_SIZE,
            M64P_GL_ALPHA_SIZE,
            M64P_GL_SWAP_CONTROL,
            M64P_GL_MULTISAMPLEBUFFERS,
            M64P_GL_MULTISAMPLESAMPLES,
            M64P_GL_CONTEXT_MAJOR_VERSION,
            M64P_GL_CONTEXT_MINOR_VERSION,
            M64P_GL_CONTEXT_PROFILE_MASK
        }


        public struct EmulatorSize
        {
            public uint uiWidth;
            public uint uiHeight;
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
            public delegate* unmanaged[Cdecl]<char*, delegate* unmanaged<void>> VidExtFuncGLGetProc;
            public delegate* unmanaged[Cdecl]<EmulatorGLAttributes, int, EmulatorStatus> VidExtFuncGLSetAttr;
            public delegate* unmanaged[Cdecl]<EmulatorGLAttributes, int*, EmulatorStatus> VidExtFuncGLGetAttr;
            public delegate* unmanaged[Cdecl]<EmulatorStatus> VidExtFuncGLSwapBuf;
            public delegate* unmanaged[Cdecl]<char*, EmulatorStatus> VidExtFuncSetCaption;
            public delegate* unmanaged[Cdecl]<EmulatorStatus> VidExtFuncToggleFS;
            public delegate* unmanaged[Cdecl]<int, int, EmulatorStatus> VidExtFuncResizeWindow;
            public delegate* unmanaged[Cdecl]<uint> VidExtFuncGLGetDefaultFramebuffer;
        }

        public enum PlayModes : int
        {
            Stopped = 1,
            Running = 2,
            Paused = 3,
        }
        public enum SaveStateTypes : int
        {
            Mupen64Plus,
            Project64Compressed,
            Project64Uncompressed,
        }
        public enum CoreTypes
        {
            PureInterpreter,
            CachedInterpreter,
            DynamicRecompiler,
        }
        public enum DisplayTypes
        {
            Windowed,
            ExclusiveFullscreen,
        }
    }
}
