using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;

using static M64RPFW.Models.Helpers.NativeLibHelper;

namespace M64RPFW.Models.Emulation.Core;

public static partial class Mupen64Plus
{
    #region Delegates for frontend API
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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate string DCoreErrorMessage(Error code);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreStartup(int apiVersion, string? configPath, string? dataPath,
        IntPtr debugContext, DebugCallback? debugCallback, IntPtr stateContext, StateCallback? stateCallback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreShutdown();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreAttachPlugin(PluginType pluginType, IntPtr pluginLibHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreDetachPlugin(PluginType pluginType);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private unsafe delegate Error DCoreDoCommand(Command cmd, int paramInt, void* paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreOverrideVidExt(VideoExtensionFunctions videoFunctionStruct);


    // Frontend function utilities
    // ========================================================

    private static void ResolveFrontendFunctions()
    {
        ResolveDelegate(_libHandle, out _fnCoreErrorMessage);
        ResolveDelegate(_libHandle, out _fnCoreStartup);
        ResolveDelegate(_libHandle, out _fnCoreShutdown);
        ResolveDelegate(_libHandle, out _fnCoreAttachPlugin);
        ResolveDelegate(_libHandle, out _fnCoreDetachPlugin);
        ResolveDelegate(_libHandle, out _fnCoreOverrideVidExt);
        ResolveDelegate(_libHandle, out _fnCoreDoCommand);
    }
    
    #endregion
    
    #region Video Extensions

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
                Functions = 14,
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
    private static VideoExtensionDelegates? _vidextDelegates;
    
    #endregion
    
    // Plugin delegates
    // ========================================================
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DPluginStartup(IntPtr library, IntPtr debugContext, DebugCallback? debugCallback);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DPluginShutdown();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DPluginGetVersion(out PluginType type, out int version, out int apiVersion, out string name,
        out int caps);

    private static Dictionary<PluginType, IntPtr> _pluginDict;

    // Frontend function members
    // ========================================================

    private static DCoreErrorMessage _fnCoreErrorMessage;
    private static DCoreStartup _fnCoreStartup;
    private static DCoreShutdown _fnCoreShutdown;
    private static DCoreAttachPlugin _fnCoreAttachPlugin;
    private static DCoreDetachPlugin _fnCoreDetachPlugin;
    private static DCoreDoCommand _fnCoreDoCommand;
    private static DCoreOverrideVidExt _fnCoreOverrideVidExt;
}