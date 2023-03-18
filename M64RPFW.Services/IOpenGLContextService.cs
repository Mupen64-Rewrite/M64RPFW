
using M64RPFW.Models.Types;

namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides access to an OpenGL context
/// </summary>
public interface IOpenGLContextService
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