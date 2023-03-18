using M64RPFW.Models.Emulation;

namespace M64RPFW.ViewModels.Interfaces;

/// <summary>
///     Represents view-specific portions of a video extension implementation.
/// </summary>
public interface IVidextSurfaceService
{
    void InitWindow();
    void QuitWindow();

    void SetGLAttribute(Mupen64Plus.GLAttribute attr, int value);
    int GetGLAttribute(Mupen64Plus.GLAttribute attr);

    void CreateWindow(int width, int height, int bitsPerPixel);
    void ResizeWindow(int width, int height);

    void MakeCurrent();
    void SwapBuffers();

    nint GetProcAddress(nint strSymbol);
    int GetDefaultFramebuffer();
}