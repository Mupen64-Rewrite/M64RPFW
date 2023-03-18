namespace M64RPFW.Services.Abstractions;

/// <summary>
///     An <see langword="interface" /> representing a bitmap.
/// </summary>
public interface IBitmap
{
    /// <summary>
    ///     Draws a pixel buffer to the bitmap
    /// </summary>
    /// <param name="buffer">The pixel buffer to be drawn in BGR888 format</param>
    /// <param name="width">The pixel buffer's width</param>
    /// <param name="height">The pixel buffer's height</param>
    void Draw(Array buffer, int width, int height);
}