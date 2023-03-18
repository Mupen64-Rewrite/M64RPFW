using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;

namespace M64RPFW.ViewModels.Interfaces;

/// <summary>
/// Represents view-specific portions of a video extension implementation.
/// </summary>
public interface IVidextSurfaceService
{
    void InitWindow();
    void QuitWindow();

    void SetGLAttribute(Mupen64PlusTypes.GLAttribute attr, int value);
    int GetGLAttribute(Mupen64PlusTypes.GLAttribute attr);

    void CreateWindow(int width, int height, int bitsPerPixel);
    void ResizeWindow(int width, int height);

    void MakeCurrent();
    void SwapBuffers();

    IntPtr GetProcAddress(IntPtr strSymbol);
    int GetDefaultFramebuffer();
}