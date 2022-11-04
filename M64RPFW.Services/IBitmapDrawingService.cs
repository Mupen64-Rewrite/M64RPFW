using System;

namespace M64RPFW.Services
{
    /// <summary>
    /// The default <see langword="interface"/> for a service that draws to a bitmap
    /// </summary>
    public interface IBitmapDrawingService
    {
        /// <summary>
        /// Whether this bitmap has been created and is ready for drawing
        /// </summary>
        public bool IsReady { get; }

        /// <summary>
        /// Allocates this bitmap based on the specified dimensions
        /// </summary>
        /// <param name="width">The bitmap's width</param>
        /// <param name="height">The bitmap's height</param>
        public void Create(int width, int height);

        /// <summary>
        /// Draws a pixel buffer to the bitmap
        /// </summary>
        /// <param name="buffer">The pixel buffer to be drawn, as a <see cref="int"/>[]</param>
        /// <param name="width">The pixel buffer's width</param>
        /// <param name="height">The pixel buffer's height</param>
        public void Draw(Array buffer, int width, int height);
    }
}
