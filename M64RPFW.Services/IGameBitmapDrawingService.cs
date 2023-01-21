namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that draws the game screen to a bitmap
/// </summary>
public interface IGameBitmapDrawingService
{
    /// <summary>
    ///     Whether this bitmap has been created and is ready for drawing
    /// </summary>
    public bool IsReady { get; }

    /// <summary>
    ///     Allocates this bitmap based on the specified dimensions
    /// </summary>
    /// <param name="width">The bitmap's width</param>
    /// <param name="height">The bitmap's height</param>
    /// <remarks>
    ///     Any previously allocated bitmap buffers are expected to be properly recreated and any size change should be accounted for in the View
    /// </remarks>
    public void Create(int width, int height);

    /// <summary>
    ///     Draws a pixel buffer to the bitmap
    /// </summary>
    /// <param name="buffer">The pixel buffer to be drawn in BGR888 format</param>
    /// <param name="width">The pixel buffer's width</param>
    /// <param name="height">The pixel buffer's height</param>
    public void Draw(Array buffer, int width, int height);
}