using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace M64RPFW.Models.Emulation.Core;

public static partial class Mupen64Plus
{
    public enum Error
    {
        Success = 0,
        NotInit, /* Function is disallowed before InitMupen64Plus() is called */
        AlreadyInit, /* InitMupen64Plus() was called twice */
        Incompatible, /* API versions between components are incompatible */
        InputAssert, /* Invalid parameters for function call, such as ParamValue=NULL for GetCoreParameter() */
        InputInvalid, /* Invalid input data, such as ParamValue="maybe" for SetCoreParameter() to set a BOOL-type value */
        InputNotFound, /* The input parameter(s) specified a particular item which was not found */
        NoMemory, /* Memory allocation failed */
        Files, /* Error opening, creating, reading, or writing to a file */
        Internal, /* Internal error */
        InvalidState, /* Current program state does not allow operation */
        PluginFail, /* A plugin function returned a fatal error */
        SystemFail, /* A system function call, such as an SDL or file operation, failed */
        Unsupported, /* Function call is not supported (ie, core not built with debugger) */
        WrongType /* A given input type parameter cannot be used for desired operation */
    }

    private static void ThrowForError(Error err)
    {
        if (err == Error.Success)
            return;
        string msg = _fnCoreErrorMessage(err);
        switch (err)
        {
            case Error.NotInit:
            case Error.AlreadyInit:
            case Error.InvalidState:
            case Error.Incompatible:
                throw new InvalidOperationException($"M64+: {msg}");
            case Error.InputAssert:
            case Error.InputInvalid:
            case Error.InputNotFound:
            case Error.WrongType:
                throw new ArgumentException($"M64+: {msg}");
            case Error.PluginFail:
            case Error.SystemFail:
            case Error.Internal:
            case Error.Files:
                throw new ApplicationException($"M64+: {msg}");
            case Error.NoMemory:
                throw new OutOfMemoryException($"M64+: {msg}");
        }
    }

    public enum MessageLevel
    {
        Error = 1,
        Warning,
        Info,
        Status,
        Verbose
    }


    public enum PluginType
    {
        Null = 0,
        RSP = 1,
        Graphics,
        Audio,
        Input,
        Core
    }

    public enum Command
    {
        NoOp = 0,
        RomOpen,
        RomClose,
        RomGetHeader,
        RomGetSettings,
        Execute,
        Stop,
        Pause,
        Resume,
        CoreStateQuery,
        StateLoad,
        StateSave,
        StateSetSlot,
        SendSDLKeydown,
        SendSDLKeyup,
        SetFrameCallback,
        TakeNextScreenshot,
        CoreStateSet,
        ReadScreen,
        Reset,
        AdvanceFrame,
    }

    public enum CoreParam
    {
        EmuState = 1,
        VideoMode,
        SavestateSlot,
        SpeedFactor,
        SpeedLimiter,
        VideoSize,
        AudioVolume,
        AudioMute,
        InputGameshark,
        StateLoadComplete,
        StateSaveComplete
    }

    public enum VideoMode
    {
        None = 1,
        Windowed,
        Fullscreen
    }


    public enum EmuState
    {
        Stopped = 1,
        Running,
        Paused
    }

    public enum Type
    {
        Int = 1,
        Float,
        Bool,
        String
    }

    public static System.Type MapToSystemType(Type t)
    {
        return t switch
        {
            Type.Int => typeof(int),
            Type.Float => typeof(float),
            Type.Bool => typeof(bool),
            Type.String => typeof(string), 
            _ => throw new ArgumentException("Invalid type", nameof(t))
        };
    }

    public enum SystemType
    {
        NTSC = 0,
        PAL,
        MPAL
    }

    public enum VideoFlags
    {
        SupportResizing = 1
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RomHeader
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
    public struct RomSettings
    {
        [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)]
        public string goodname;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
        public byte[] MD5;

        public byte savetype;
        public byte status; // Rom status on a scale from 0-5. 
        public byte players; // Local players 0-4, 2/3/4 way Netplay indicated by 5/6/7. 
        public byte rumble; // 0 - No, 1 - Yes boolean for rumble support. 
        public byte transferpak; // 0 - No, 1 - Yes boolean for transfer pak support. 
        public byte mempak; // 0 - No, 1 - Yes boolean for memory pak support. 
        public byte biopak; // 0 - No, 1 - Yes boolean for bio pak support. 
    }

    public enum GLAttribute
    {
        DoubleBuffer = 1,
        BufferSize,
        DepthSize,
        RedSize,
        GreenSize,
        BlueSize,
        AlphaSize,
        SwapControl,
        MultisampleBuffers,
        MultisampleSamples,
        ContextMajorVersion,
        ContextMinorVersion,
        ContextProfileMask
    }

    public enum GLContextType
    {
        Core = 0,
        Compatibilty,
        ES
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Size2D
    {
        public uint uiWidth;
        public uint uiHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VideoExtensionFunctions
    {
        public uint Functions;
        public IntPtr VidExtFuncInit;
        public IntPtr VidExtFuncQuit;
        public IntPtr VidExtFuncListModes;
        public IntPtr VidExtFuncListRates;
        public IntPtr VidExtFuncSetMode;
        public IntPtr VidExtFuncSetModeWithRate;
        public IntPtr VidExtFuncGLGetProc;
        public IntPtr VidExtFuncGLSetAttr;
        public IntPtr VidExtFuncGLGetAttr;
        public IntPtr VidExtFuncGLSwapBuf;
        public IntPtr VidExtFuncSetCaption;
        public IntPtr VidExtFuncToggleFS;
        public IntPtr VidExtFuncResizeWindow;
        public IntPtr VidExtFuncGLGetDefaultFramebuffer;

        public static VideoExtensionFunctions Empty =>
            new VideoExtensionFunctions
            {
                Functions = 14,
                VidExtFuncInit = IntPtr.Zero,
                VidExtFuncQuit = IntPtr.Zero,
                VidExtFuncListModes = IntPtr.Zero,
                VidExtFuncListRates = IntPtr.Zero,
                VidExtFuncSetMode = IntPtr.Zero,
                VidExtFuncSetModeWithRate = IntPtr.Zero,
                VidExtFuncResizeWindow = IntPtr.Zero,
                VidExtFuncSetCaption = IntPtr.Zero,
                VidExtFuncToggleFS = IntPtr.Zero,
                VidExtFuncGLGetProc = IntPtr.Zero,
                VidExtFuncGLSetAttr = IntPtr.Zero,
                VidExtFuncGLGetAttr = IntPtr.Zero,
                VidExtFuncGLSwapBuf = IntPtr.Zero,
                VidExtFuncGLGetDefaultFramebuffer = IntPtr.Zero
            };
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FrameCallback(int index);

    // Custom

    public enum PlayModes : int
    {
        None,
        Stopped,
        Running,
        Paused,
    }

    public enum SavestateType : int
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