using M64RPFW.Models.Types;

namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides Mupen64plus VidExt functionality
/// </summary>
public interface IVideoExtensionService
{
    /// <summary>
    /// Initializes the VidExt subsystem
    /// </summary>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextInit();
    
    /// <summary>
    /// Destroys the VidExt subsystem
    /// </summary>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextQuit();
    
    /// <summary>
    /// Lists available monitor sizes
    /// </summary>
    /// <param name="sizes">A span of sizes</param>
    /// <param name="len">The length of the <paramref name="sizes"/></param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextListFullscreenModes(Span<Mupen64PlusTypes.Size2D> sizes, ref int len);
    
    /// <summary>
    /// Lists available refresh rates for the specified size
    /// </summary>
    /// <param name="size">The size</param>
    /// <param name="output">A span of refresh rates</param>
    /// <param name="len">The length of the <paramref name="output"/></param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextListFullscreenRates(Mupen64PlusTypes.Size2D size, Span<int> output, ref int len);
    
    /// <summary>
    /// Sets the video mode
    /// </summary>
    /// <param name="width">The screen's width</param>
    /// <param name="height">">The screen's height</param>
    /// <param name="bpp">The bits per pixel (expected: 32)</param>
    /// <param name="mode">The video mode</param>
    /// <param name="flags">The video flags</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextSetVideoMode(int width, int height, int bpp, Mupen64PlusTypes.VideoMode mode, Mupen64PlusTypes.VideoFlags flags);
    
    /// <summary>
    /// Sets the video mode and alters the refresh rate
    /// </summary>
    ///  /// <param name="width">The screen's width</param>
    /// <param name="height">">The screen's height</param>
    /// <param name="refreshRate">The refresh rate</param>
    /// <param name="bpp">The bits per pixel (expected: 32)</param>
    /// <param name="mode">The video mode</param>
    /// <param name="flags">The video flags</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextSetVideoModeWithRate(int width, int height, int refreshRate, int bpp,
        Mupen64PlusTypes.VideoMode mode, Mupen64PlusTypes.VideoFlags flags);

    /// <summary>
    /// Gets a pointer to the OpenGL function associated with the specified symbol
    /// </summary>
    /// <param name="symbol">The symbol to look for</param>
    /// <returns>The operation's result</returns>
    unsafe IntPtr VidextGLGetProcAddress(byte* symbol);
    
    /// <summary>
    /// Sets an OpenGL attribute
    /// </summary>
    /// <param name="attr">The OpenGL attribute</param>
    /// <param name="value">The new value</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextGLSetAttr(Mupen64PlusTypes.GLAttribute attr, int value);
    
    /// <summary>
    /// Gets an OpenGL attribute's value
    /// </summary>
    /// <param name="attr">The OpenGL attribute</param>
    /// <param name="value">The OpenGL attribute's value</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextGLGetAttr(Mupen64PlusTypes.GLAttribute attr, out int value);

    /// <summary>
    /// Sets the window's size
    /// </summary>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextResizeWindow(int width, int height);
    
    /// <summary>
    /// Sets the window's caption
    /// </summary>
    /// <param name="str">The new caption</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextSetCaption(string str);
    
    /// <summary>
    /// Toggles the full-screen mode
    /// </summary>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextToggleFullscreen();
    
    /// <summary>
    /// Swaps the front and back buffers
    /// </summary>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextSwapBuffers();
    
    /// <summary>
    /// Gets a pointer to the default framebuffer
    /// </summary>
    /// <returns>A pointer to the default framebuffer</returns>
    uint VidextGLGetDefaultFramebuffer();
}