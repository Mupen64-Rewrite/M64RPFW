using System.Runtime.InteropServices;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Services;

namespace M64RPFW.Models.Interfaces;

public static class VideoExtensionServiceExtensions
{
    /// <summary>
    /// Extracts delegates from an <see cref="IVideoExtensionService"/>.
    /// </summary>
    /// <param name="service">The service to extract bindings from</param>
    /// <returns>A class that can be passed to <see cref="Mupen64Plus.OverrideVidExt"/></returns>
    public static unsafe Mupen64PlusTypes.VideoExtensionFunctions ToVidextStruct(this IVideoExtensionService service)
    {
        return new Mupen64PlusTypes.VideoExtensionFunctions
        {
            Functions = 14,
            VidExtFuncInit = service.VidextInit,
            VidExtFuncQuit = service.VidextQuit,
            VidExtFuncListModes = service.VidextListFullscreenModes,
            VidExtFuncListRates = service.VidextListFullscreenRates,
            VidExtFuncSetMode = service.VidextSetVideoMode,
            VidExtFuncSetModeWithRate = service.VidextSetVideoModeWithRate,
            VidExtFuncGLGetProc = service.VidextGLGetProcAddress,
            VidExtFuncGLSetAttr = service.VidextGLSetAttr,
            VidExtFuncGLGetAttr = service.VidextGLGetAttr,
            VidExtFuncResizeWindow = service.VidextResizeWindow,
            VidExtFuncSetCaption = str => service.VidextSetCaption(Marshal.PtrToStringAnsi((IntPtr) str) ?? ""),
            VidExtFuncToggleFS = service.VidextToggleFullscreen,
            VidExtFuncGLSwapBuf = service.VidextSwapBuffers,
            VidExtFuncGLGetDefaultFramebuffer = service.VidextGLGetDefaultFramebuffer
        };
    }
}