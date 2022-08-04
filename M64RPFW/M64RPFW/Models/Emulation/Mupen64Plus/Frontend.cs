using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using M64PRR.Models.Helpers;

namespace M64RPFW.Models.Emulation.Mupen64Plus;

public partial class Mupen64Plus
{
    // Callback types for the frontend API
    // ========================================================

    /// <summary>
    /// Logs a message from Mupen64Plus or an attached plugin.
    /// </summary>
    /// <param name="context">the pointer provided as debugContext to CoreStartup</param>
    /// <param name="level">the message's logging level</param>
    /// <param name="message">the message to log</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugCallback(IntPtr context, MessageLevel level, string message);

    /// <summary>
    /// Responds to a Mupen64Plus core parameter changing.
    /// </summary>
    /// <param name="context">the pointer provided as stateContext to CoreStartup</param>
    /// <param name="param">the core parameter that changed</param>
    /// <param name="newValue">the core parameter's new value</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void StateCallback(IntPtr context, CoreParam param, int newValue);

    // Delegate types for core functions
    // ========================================================

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DCoreStartup(int apiVersion, string? configPath, string? dataPath,
        IntPtr debugContext, DebugCallback? debugCallback, IntPtr stateContext, StateCallback? stateCallback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DCoreShutdown();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DCoreAttachPlugin(PluginType pluginType, IntPtr pluginLibHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DCoreDetachPlugin(PluginType pluginType);

    /// <summary>
    /// A delegate for the M64+ function <c>CoreDoCommand</c>.
    /// Used for passing in values through paramPtr.
    /// </summary>
    /// <typeparam name="TP">The type to marshal as the pointer parameter.</typeparam>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DCoreDoCommand<in TP>(Command command, int paramInt, TP paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DCoreDoCommand_Ref<TP>(Command command, int paramInt, ref TP paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DCoreOverrideVidExt(VideoExtensionFunctions videoFunctionStruct);


    // Frontend function utilities
    // ========================================================

    private void ResolveFrontendFunctions()
    {
        _fnCoreStartup = NativeLibHelper.GetFunction<DCoreStartup>(_libHandle, "CoreStartup");
        _fnCoreShutdown = NativeLibHelper.GetFunction<DCoreShutdown>(_libHandle, "CoreShutdown");
        _fnCoreAttachPlugin = NativeLibHelper.GetFunction<DCoreAttachPlugin>(_libHandle, "CoreAttachPlugin");
        _fnCoreDetachPlugin = NativeLibHelper.GetFunction<DCoreDetachPlugin>(_libHandle, "CoreDetachPlugin");
        _fnCoreDoCommand = NativeLibrary.GetExport(_libHandle, "CoreDoCommand");
        _fnCoreOverrideVidExt = NativeLibHelper.GetFunction<DCoreOverrideVidExt>(_libHandle, "CoreOverrideVidExt");
    }

    private Error CoreDoCommand(Command command, int paramInt, IntPtr paramPtr)
    {
        var fn = Marshal.GetDelegateForFunctionPointer<DCoreDoCommand<IntPtr>>(_fnCoreDoCommand);
        return fn(command, paramInt, paramPtr);
    }

    private Error CoreDoCommand(Command command, int paramInt, string paramPtr)
    {
        IntPtr cstr = Marshal.StringToHGlobalAnsi(paramPtr);
        try
        {
            return CoreDoCommand(command, paramInt, cstr);
        }
        finally
        {
            Marshal.FreeHGlobal(cstr);
        }
    }

    private Error CoreDoCommand<T>(Command command, int paramInt, T[] paramPtr)
    {
        var fn = Marshal.GetDelegateForFunctionPointer<DCoreDoCommand<T[]>>(_fnCoreDoCommand);
        return fn(command, paramInt, paramPtr);
    }

    private Error CoreDoCommand<T>(Command command, int paramInt, ref T paramPtr)
    {
        var fn = Marshal.GetDelegateForFunctionPointer<DCoreDoCommand_Ref<T>>(_fnCoreDoCommand);
        return fn(command, paramInt, ref paramPtr);
    }

    // Video extension handling
    // ========================================================

    public interface IVideoExtension
    {
        Error Init();
        Error Quit();
        Error ListFullscreenModes();
        Error ListFullscreenRates(Size2D size);
        Error SetVideoMode(int width, int height, int bitsPerPixel, VideoMode mode, VideoFlags flags);

        Error SetVideoModeWithRate(int width, int height, int refreshRate, int bitsPerPixel, VideoMode mode,
            VideoFlags flags);

        Error ResizeWindow(Size2D size);
        Error SetCaption(string title);
        Error ToggleFullScreen();

        IntPtr GLGetProcAddress(string symbol);
        Error SetAttribute(GLAttribute attr, int value);
        Error GetAttribute(GLAttribute attr, out int value);
        Error SwapBuffers();
        uint GetDefaultFramebuffer();
    }

    private class VideoExtensionDelegates
    {
        public delegate Error DVidExt_Init();

        public delegate Error DVidExt_Quit();

        public delegate Error DVidExt_ListFullscreenModes();

        public delegate Error DVidExt_ListFullscreenRates(Size2D size);

        public delegate Error DVidExt_SetVideoMode(int width, int height, int bitsPerPixel, VideoMode mode,
            VideoFlags flags);

        public delegate Error DVidExt_SetVideoModeWithRate(int width, int height, int refreshRate, int bitsPerPixel,
            VideoMode mode,
            VideoFlags flags);

        public delegate Error DVidExt_ResizeWindow(Size2D size);

        public delegate Error DVidExt_SetCaption(string title);

        public delegate Error DVidExt_ToggleFullScreen();

        public delegate IntPtr DVidExt_GLGetProcAddress(string symbol);

        public delegate Error DVidExt_SetAttribute(GLAttribute attr, int value);

        public delegate Error DVidExt_GetAttribute(GLAttribute attr, out int value);

        public delegate Error DVidExt_SwapBuffers();

        public delegate uint DVidExt_GetDefaultFramebuffer();

        public VideoExtensionDelegates(IVideoExtension vidextObj)
        {
            InitFn = vidextObj.Init;
            QuitFn = vidextObj.Quit;
            ListFullscreenModesFn = vidextObj.ListFullscreenModes;
            ListFullscreenRatesFn = vidextObj.ListFullscreenRates;
            SetVideoModeFn = vidextObj.SetVideoMode;

            SetVideoModeWithRateFn = vidextObj.SetVideoModeWithRate;

            ResizeWindowFn = vidextObj.ResizeWindow;
            SetCaptionFn = vidextObj.SetCaption;
            ToggleFullScreenFn = vidextObj.ToggleFullScreen;

            GLGetProcAddressFn = vidextObj.GLGetProcAddress;
            SetAttributeFn = vidextObj.SetAttribute;
            GetAttributeFn = vidextObj.GetAttribute;
            SwapBuffersFn = vidextObj.SwapBuffers;
            GetDefaultFramebufferFn = vidextObj.GetDefaultFramebuffer;
        }

        public VideoExtensionFunctions AsNative() 
        {
            return new VideoExtensionFunctions
            {
                Functions = 11,
                VidExtFuncInit = Marshal.GetFunctionPointerForDelegate(InitFn),
                VidExtFuncQuit = Marshal.GetFunctionPointerForDelegate(QuitFn),
                VidExtFuncListModes = Marshal.GetFunctionPointerForDelegate(ListFullscreenModesFn),
                VidExtFuncListRates = Marshal.GetFunctionPointerForDelegate(ListFullscreenRatesFn),
                VidExtFuncSetMode = Marshal.GetFunctionPointerForDelegate(SetVideoModeFn),
                VidExtFuncSetModeWithRate = Marshal.GetFunctionPointerForDelegate(SetVideoModeWithRateFn),
                VidExtFuncResizeWindow = Marshal.GetFunctionPointerForDelegate(ResizeWindowFn),
                VidExtFuncSetCaption = Marshal.GetFunctionPointerForDelegate(SetCaptionFn),
                VidExtFuncToggleFS = Marshal.GetFunctionPointerForDelegate(ToggleFullScreenFn),
                VidExtFuncGLGetProc = Marshal.GetFunctionPointerForDelegate(GLGetProcAddressFn),
                VidExtFuncGLSetAttr = Marshal.GetFunctionPointerForDelegate(SetAttributeFn),
                VidExtFuncGLGetAttr = Marshal.GetFunctionPointerForDelegate(GetAttributeFn),
                VidExtFuncGLSwapBuf = Marshal.GetFunctionPointerForDelegate(SwapBuffersFn),
                VidExtFuncGLGetDefaultFramebuffer = Marshal.GetFunctionPointerForDelegate(GetDefaultFramebufferFn),
            };
        }

        public DVidExt_Init InitFn;
        public DVidExt_Quit QuitFn;
        public DVidExt_ListFullscreenModes ListFullscreenModesFn;
        public DVidExt_ListFullscreenRates ListFullscreenRatesFn;
        public DVidExt_SetVideoMode SetVideoModeFn;

        public DVidExt_SetVideoModeWithRate SetVideoModeWithRateFn;

        public DVidExt_ResizeWindow ResizeWindowFn;
        public DVidExt_SetCaption SetCaptionFn;
        public DVidExt_ToggleFullScreen ToggleFullScreenFn;

        public DVidExt_GLGetProcAddress GLGetProcAddressFn;
        public DVidExt_SetAttribute SetAttributeFn;
        public DVidExt_GetAttribute GetAttributeFn;
        public DVidExt_SwapBuffers SwapBuffersFn;
        public DVidExt_GetDefaultFramebuffer GetDefaultFramebufferFn;
    }

    private IVideoExtension? _vidextObject;
    private VideoExtensionDelegates? _vidextDelegates;

    // Frontend function members
    // ========================================================

    private DCoreStartup _fnCoreStartup;
    private DCoreShutdown _fnCoreShutdown;
    private DCoreAttachPlugin _fnCoreAttachPlugin;
    private DCoreDetachPlugin _fnCoreDetachPlugin;
    private IntPtr _fnCoreDoCommand;
    private DCoreOverrideVidExt _fnCoreOverrideVidExt;
}