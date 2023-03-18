using M64RPFW.Models.Types;

namespace M64RPFW.Services;

public interface IVideoExtensionService
{
    Mupen64PlusTypes.Error VidextInit();
    Mupen64PlusTypes.Error VidextQuit();
    
    Mupen64PlusTypes.Error VidextListFullscreenModes(Span<Mupen64PlusTypes.Size2D> sizes, ref int len);
    Mupen64PlusTypes.Error VidextListFullscreenRates(Mupen64PlusTypes.Size2D size, Span<int> output, ref int len);
    
    Mupen64PlusTypes.Error VidextSetVideoMode(int width, int height, int bpp, Mupen64PlusTypes.VideoMode mode, Mupen64PlusTypes.VideoFlags flags);
    Mupen64PlusTypes.Error VidextSetVideoModeWithRate(int width, int height, int refreshRate, int bpp,
        Mupen64PlusTypes.VideoMode mode, Mupen64PlusTypes.VideoFlags flags);

    unsafe IntPtr VidextGLGetProcAddress(byte* symbol);
    Mupen64PlusTypes.Error VidextGLSetAttr(Mupen64PlusTypes.GLAttribute attr, int value);
    Mupen64PlusTypes.Error VidextGLGetAttr(Mupen64PlusTypes.GLAttribute attr, out int value);

    Mupen64PlusTypes.Error VidextResizeWindow(int width, int height);
    Mupen64PlusTypes.Error VidextSetCaption(string str);
    Mupen64PlusTypes.Error VidextToggleFullscreen();
    
    Mupen64PlusTypes.Error VidextSwapBuffers();
    uint VidextGLGetDefaultFramebuffer();
}