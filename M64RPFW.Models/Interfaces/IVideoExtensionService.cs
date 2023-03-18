using System.Runtime.InteropServices;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;

namespace M64RPFW.Models.Interfaces;

public unsafe interface IVideoExtensionService
{
    Mupen64PlusTypes.Error VidextInit();
    Mupen64PlusTypes.Error VidextQuit();
    
    Mupen64PlusTypes.Error VidextListFullscreenModes(Span<Mupen64PlusTypes.Size2D> sizes, ref int len);
    Mupen64PlusTypes.Error VidextListFullscreenRates(Mupen64PlusTypes.Size2D size, Span<int> output, ref int len);
    
    Mupen64PlusTypes.Error VidextSetVideoMode(int width, int height, int bpp, Mupen64PlusTypes.VideoMode mode, Mupen64PlusTypes.VideoFlags flags);
    Mupen64PlusTypes.Error VidextSetVideoModeWithRate(int width, int height, int refreshRate, int bpp,
        Mupen64PlusTypes.VideoMode mode, Mupen64PlusTypes.VideoFlags flags);

    IntPtr VidextGLGetProcAddress(byte* symbol);
    Mupen64PlusTypes.Error VidextGLSetAttr(Mupen64PlusTypes.GLAttribute attr, int value);
    Mupen64PlusTypes.Error VidextGLGetAttr(Mupen64PlusTypes.GLAttribute attr, out int value);

    Mupen64PlusTypes.Error VidextResizeWindow(int width, int height);
    Mupen64PlusTypes.Error VidextSetCaption(string str);
    Mupen64PlusTypes.Error VidextToggleFullscreen();
    
    Mupen64PlusTypes.Error VidextSwapBuffers();
    uint VidextGLGetDefaultFramebuffer();
}

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