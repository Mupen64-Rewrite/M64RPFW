using System;
using System.Runtime.InteropServices;
using Mupen64PlusRR.Models.Emulation;

namespace Mupen64PlusRR.Models.Interfaces;

public unsafe interface IVideoExtensionService
{
    Mupen64Plus.Error VidextInit();
    Mupen64Plus.Error VidextQuit();
    
    Mupen64Plus.Error VidextListFullscreenModes(Span<Mupen64Plus.Size2D> sizes, ref int len);
    Mupen64Plus.Error VidextListFullscreenRates(Mupen64Plus.Size2D size, Span<int> output, ref int len);
    
    Mupen64Plus.Error VidextSetVideoMode(int width, int height, int bpp, Mupen64Plus.VideoMode mode, Mupen64Plus.VideoFlags flags);
    Mupen64Plus.Error VidextSetVideoModeWithRate(int width, int height, int refreshRate, int bpp,
        Mupen64Plus.VideoMode mode, Mupen64Plus.VideoFlags flags);

    IntPtr VidextGLGetProcAddress(byte* symbol);
    Mupen64Plus.Error VidextGLSetAttr(Mupen64Plus.GLAttribute attr, int value);
    Mupen64Plus.Error VidextGLGetAttr(Mupen64Plus.GLAttribute attr, out int value);

    Mupen64Plus.Error VidextResizeWindow(int width, int height);
    Mupen64Plus.Error VidextSetCaption(string str);
    Mupen64Plus.Error VidextToggleFullscreen();
    
    Mupen64Plus.Error VidextSwapBuffers();
    uint VidextGLGetDefaultFramebuffer();
}

public static class VideoExtensionServiceExtensions
{
    /// <summary>
    /// Extracts delegates from an <see cref="IVideoExtensionService"/>.
    /// </summary>
    /// <param name="service">The service to extract bindings from</param>
    /// <returns>A class that can be passed to <see cref="Mupen64Plus.OverrideVidExt"/></returns>
    public static unsafe Mupen64Plus.VideoExtensionFunctions ToVidextStruct(this IVideoExtensionService service)
    {
        return new Mupen64Plus.VideoExtensionFunctions
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