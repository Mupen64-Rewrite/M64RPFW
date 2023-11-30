using SkiaSharp;

namespace M64RPFW.ViewModels.Scripting.Graphics;

public record FillPaintParams(float Red, float Green, float Blue, float Alpha);

public record StrokePaintParams(float StrokeWidth, float Red, float Green, float Blue, float Alpha);

public record TextPaintParams(string FontName, float FontSize, int FontWeight, int FontStyle,
    float Red, float Green, float Blue, float Alpha);

public record ImagePaintParams(int Interpolation, float Opacity);

public static class PaintFactories
{
    public static SKPaint MakeFillPaint(FillPaintParams key)
    {
        return new SKPaint
        {
            Style = SKPaintStyle.Fill,
            ColorF = new SKColorF(key.Red, key.Green, key.Blue, key.Alpha)
        };
    }
    
    public static SKPaint MakeStrokePaint(StrokePaintParams key)
    {
        return new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = key.StrokeWidth,
            ColorF = new SKColorF(key.Red, key.Green, key.Blue, key.Alpha)
        };
    }
    
    public static SKPaint MakeTextPaint(TextPaintParams key)
    {
        return new SKPaint(new SKFont(SKTypeface.FromFamilyName(
                    familyName: key.FontName,
                    weight: (SKFontStyleWeight) key.FontWeight,
                    width: SKFontStyleWidth.Normal,
                    slant: key.FontStyle switch
                    {
                        0 => SKFontStyleSlant.Upright,
                        1 => SKFontStyleSlant.Oblique,
                        2 => SKFontStyleSlant.Italic,
                        _ => throw new ArgumentException($"Invalid style value {key.FontStyle}")
                    }
                ), key.FontSize
            )
        )
        {
            ColorF = new SKColorF(key.Red, key.Green, key.Blue, key.Alpha),
            
        };
    }

    public static SKPaint MakeImagePaint(ImagePaintParams key)
    {
        return new SKPaint
        {
            FilterQuality = key.Interpolation switch
            {
                0 => SKFilterQuality.None,
                1 => SKFilterQuality.Medium,
                _ => SKFilterQuality.Medium
            },
            // multiply alpha by opacity. (@formatter:off)
            ColorFilter = SKColorFilter.CreateColorMatrix(new []
            {
               1.0f,  0.0f,  0.0f,  0.0f,     0.0f, 
               0.0f,  1.0f,  0.0f,  0.0f,     0.0f, 
               0.0f,  0.0f,  1.0f,  0.0f,     0.0f, 
               0.0f,  0.0f,  0.0f,  key.Opacity,  0.0f, 
            })
            // @formatter:on
        };
    }
}