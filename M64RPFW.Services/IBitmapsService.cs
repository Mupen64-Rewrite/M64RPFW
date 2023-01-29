using M64RPFW.Services.Abstractions;

namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that creates bitmaps
/// </summary>
public interface IBitmapsService
{
    /// <summary>
    ///     Targets, which can be used as a drawing surface for a bitmap
    /// </summary>
    public enum BitmapTargets
    {
        /// <summary>
        ///     An invalid render target
        /// </summary>
        Invalid,
        
        /// <summary>
        ///     The game/emulator viewport
        /// </summary>
        Game,
    }

    /// <summary>
    ///     Creates a new bitmap with the specified dimensions
    /// </summary>
    /// <param name="bitmapTarget">The bitmap's target</param>
    /// <param name="width">The bitmap's width</param>
    /// <param name="height">The bitmap's height</param>
    /// <returns>The created bitmap</returns>
    IBitmap Create(BitmapTargets bitmapTarget, int width, int height);

    
}