using M64RPFW.Models.Types;

namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides access to an OpenGL context
/// </summary>
public interface IOpenGLContextService
{
    /// <summary>
    ///     Initializes the window.
    /// </summary>
    void InitWindow();

    /// <summary>
    ///     Destroys the window.
    /// </summary>
    void QuitWindow();

    /// <summary>
    ///     Sets an OpenGL attribute's value.
    /// </summary>
    /// <param name="attr">The OpenGL attribute</param>
    /// <param name="value">The new value</param>
    void SetGLAttribute(Mupen64PlusTypes.GLAttribute attr, int value);

    /// <summary>
    ///     Gets an OpenGL attribute's value.
    /// </summary>
    /// <param name="attr">The OpenGL attribute</param>
    /// <returns>The attribute's value</returns>
    int GetGLAttribute(Mupen64PlusTypes.GLAttribute attr);

    /// <summary>
    ///     Creates the window.
    /// </summary>
    /// <param name="width">The window's width</param>
    /// <param name="height">The window's height</param>
    /// <param name="bitsPerPixel">The amount of bits used to represent one pixel</param>
    void CreateGlWindow(int width, int height, int bitsPerPixel);

    /// <summary>
    ///     Notifies the window that it's being resized.
    /// </summary>
    /// <param name="width">The window's width</param>
    /// <param name="height">The window's height</param>
    void ResizeGlWindow(int width, int height);

    /// <summary>
    ///     Makes this window's OpenGL context current on this thread.
    /// </summary>
    void MakeCurrent();

    /// <summary>
    ///     Swaps the front and back buffers
    /// </summary>
    void SwapBuffers();

    /// <summary>
    ///     Gets an OpenGL symbol's address.
    /// </summary>
    /// <param name="strSymbol">A pointer to the desired symbol's name</param>
    /// <returns>A pointer to the symbol</returns>
    nint GetProcAddress(nint strSymbol);

    /// <summary>
    ///     Returns the default OpenGL framebuffer's ID.
    /// </summary>
    /// <returns>the default OpenGL framebuffer's ID</returns>
    /// <remarks>
    ///     Might be non-0 on some devices
    /// </remarks>
    uint GetDefaultFramebuffer();
}