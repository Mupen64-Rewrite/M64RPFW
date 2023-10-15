using SkiaSharp;

namespace M64RPFW.ViewModels.Scripting.Extensions;

/// <summary>
/// A class providing extension methods and helpers for <see cref="SkiaSharp"/>
/// </summary>
internal static class SkiaExtensions
{
    /// <summary>
    /// Creates an <see cref="SKColor"/> from float channels
    /// </summary>
    /// <param name="r">The red channel</param>
    /// <param name="g">The green channel</param>
    /// <param name="b">The blue channel</param>
    /// <param name="a">The alpha channel</param>
    /// <returns>An <see cref="SKColor"/> from float channels</returns>
    public static SKColor ColorFromFloats(float r, float g, float b, float a)
    {
        return new SKColor((byte)(r * byte.MaxValue), (byte)(g * byte.MaxValue), (byte)(b * byte.MaxValue), (byte)(a * byte.MaxValue));
    }
}