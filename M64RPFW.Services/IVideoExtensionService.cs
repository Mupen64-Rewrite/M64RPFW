using System.Runtime.InteropServices;
using M64RPFW.Models.Types;

namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides Mupen64plus VidExt functionality
/// </summary>
public interface IVideoExtensionService
{
    /// <summary>
    /// Initializes the VidExt subsystem.
    /// </summary>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextInit();

    /// <summary>
    /// Destroys the VidExt subsystem.
    /// </summary>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextQuit();

    /// <summary>
    /// Lists available fullscreen monitor sizes.
    /// </summary>
    /// <param name="sizes">A span of sizes</param>
    /// <param name="len">The length of the <paramref name="sizes"/> span. Will be set to the actual number of sizes after returning.</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextListFullscreenModes(Span<Mupen64PlusTypes.Size2D> sizes, ref int len);

    /// <summary>
    /// Lists available fullscreen refresh rates for the specified size
    /// </summary>
    /// <param name="size">The size</param>
    /// <param name="output">A span to store the outputted values</param>
    /// <param name="len">The <paramref name="output"/>'s length. Will be set to the actual number of rates after returning.</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextListFullscreenRates(Mupen64PlusTypes.Size2D size, Span<int> output, ref int len);

    /// <summary>
    /// Initializes the OpenGL window with the specified size, bit depth, mode, and flags.
    /// Graphics-related attributes may be set via <see cref="VidextGLSetAttr"/>.
    /// </summary>
    /// <param name="width">The screen's width</param>
    /// <param name="height">">The screen's height</param>
    /// <param name="bpp">The bits per pixel (expected: 32)</param>
    /// <param name="mode">The video mode</param>
    /// <param name="flags">The video flags</param>
    /// <returns>The operation's result</returns>
    /// <seealso cref="VidextSetVideoModeWithRate"/>
    Mupen64PlusTypes.Error VidextSetVideoMode(int width, int height, int bpp, Mupen64PlusTypes.VideoMode mode,
        Mupen64PlusTypes.VideoFlags flags);

    /// <summary>
    /// Initializes the OpenGL window with the specified size, refresh rate, bit depth, mode, and flags.
    /// Graphics-related attributes may be set via <see cref="VidextGLSetAttr"/>
    /// </summary>
    /// <param name="width">The screen's width</param>
    /// <param name="height">">The screen's height</param>
    /// <param name="refreshRate">The refresh rate</param>
    /// <param name="bpp">The bits per pixel (expected: 32)</param>
    /// <param name="mode">The video mode</param>
    /// <param name="flags">The video flags</param>
    /// <returns>The operation's result</returns>
    /// <seealso cref="VidextSetVideoMode"/>
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
    /// Notifies the VidExt subsystem that the window has been resized.
    /// </summary>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextResizeWindow(int width, int height);

    /// <summary>
    /// Sets the window's title. This may do nothing if it is not desired for
    /// the graphics plugin to do so.
    /// </summary>
    /// <param name="str">The new caption</param>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextSetCaption(string str);

    /// <summary>
    /// Switches between fullscreen and windowed mode.
    /// </summary>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextToggleFullscreen();

    /// <summary>
    /// Swaps the front and back buffers.
    /// </summary>
    /// <returns>The operation's result</returns>
    Mupen64PlusTypes.Error VidextSwapBuffers();

    /// <summary>
    /// Returns the default OpenGL framebuffer's ID
    /// </summary>
    /// <returns>the default OpenGL framebuffer's ID</returns>
    uint VidextGLGetDefaultFramebuffer();
}

public static class VideoExtensionServiceExtensions
{
    public static unsafe Mupen64PlusTypes.VideoExtensionFunctions ToVidextStruct(this IVideoExtensionService s)
    {
        return new Mupen64PlusTypes.VideoExtensionFunctions
        {
            Functions = 14,
            VidExtFuncInit = s.VidextInit,
            VidExtFuncQuit = s.VidextQuit,
            VidExtFuncListModes = s.VidextListFullscreenModes,
            VidExtFuncListRates = s.VidextListFullscreenRates,
            VidExtFuncSetMode = s.VidextSetVideoMode,
            VidExtFuncSetModeWithRate = s.VidextSetVideoModeWithRate,
            VidExtFuncGLGetProc = s.VidextGLGetProcAddress,
            VidExtFuncGLSetAttr = s.VidextGLSetAttr,
            VidExtFuncGLGetAttr = s.VidextGLGetAttr,
            VidExtFuncGLSwapBuf = s.VidextSwapBuffers,
            VidExtFuncSetCaption = title =>
            {
                var str = Marshal.PtrToStringAnsi((IntPtr) title);
                if (str != null)
                    s.VidextSetCaption(str);
                return Mupen64PlusTypes.Error.Success;
            },
            VidExtFuncToggleFS = s.VidextToggleFullscreen,
            VidExtFuncResizeWindow = s.VidextResizeWindow,
            VidExtFuncGLGetDefaultFramebuffer = s.VidextGLGetDefaultFramebuffer
        };
    }
}