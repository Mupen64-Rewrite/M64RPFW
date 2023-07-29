using SkiaSharp;

namespace M64RPFW.Models.Scripting.Graphics;

public record TextLayoutParameters(float MaxWidth, float MaxHeight, string FontName, float FontSize, int FontWeight, int FontStyle,
    int HorizontalAlignment, SKColor Color);