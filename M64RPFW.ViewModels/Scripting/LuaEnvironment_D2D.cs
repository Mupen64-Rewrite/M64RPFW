using BitFaster.Caching.Lru;
using M64RPFW.ViewModels.Scripting.Extensions;
using M64RPFW.ViewModels.Scripting.Graphics;
using NLua;
using SkiaSharp;
using Topten.RichTextKit;
using SkiaExtensions = M64RPFW.ViewModels.Scripting.Extensions.SkiaExtensions;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    private const int TextLayoutCacheLimit = 1000;

    private readonly ClassicLru<TextLayoutParameters, TextBlock> _textLayoutCache = new(TextLayoutCacheLimit);
    private int _textAntialiasMode;
    private readonly Dictionary<string, SKImage> _imageDict = new();

    [LibFunction("d2d.fill_rectangle")]
    private void D2D_FillRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha)
    {
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        };
        _skCanvas?.DrawRect(x, y, right - x, bottom - y, paint);
    }

    [LibFunction("d2d.draw_rectangle")]
    private void D2D_DrawRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, float thickness)
    {
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        };
        _skCanvas?.DrawRect(x, y, right - x, bottom - y, paint);
    }

    [LibFunction("d2d.fill_ellipse")]
    private void D2D_FillEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha)
    {
        using var skPaint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        };
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, skPaint);
    }

    [LibFunction("d2d.draw_ellipse")]
    private void D2D_DrawEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha, float thickness)
    {
        // FIXME: this needs a workaround, as stroke-only ovals are seemingly not supported?
        // maybe use a path
        #if false
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        };
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, paint);
        #endif
    }

    [LibFunction("d2d.draw_line")]
    private void D2D_DrawLine(float x0, float y0, float x1, float y1, float red, float green, float blue, float alpha,
        float thickness)
    {
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        };
        _skCanvas?.DrawLine(x0, y0, x1, y1, paint);
    }


    [LibFunction("d2d.set_text_antialias_mode")]
    private void D2D_SetTextAntialiasMode(int mode)
    {
        _textAntialiasMode = mode;
    }

    [LibFunction("d2d.draw_text")]
    private void D2D_DrawText(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, string text, string fontName, float fontSize, int fontWeight, int fontStyle,
        int horizontalAlignment, int verticalAlignment, int options)
    {
        if (_skCanvas == null)
            return;

        // Cache laid-out text blocks. Unfortunately, I have to key with
        // colour, though I wish I didn't need to.
        // FIXME: Caching produced collisions, so it was disabled
        var cacheKey = new TextLayoutParameters(
            MaxWidth: right - x,
            MaxHeight: bottom - y,
            FontName: fontName,
            FontSize: fontSize,
            FontWeight: fontWeight,
            FontStyle: fontStyle,
            HorizontalAlignment: horizontalAlignment,
            Color: SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        );
        
        var block = new TextBlock
        {
            MaxWidth = cacheKey.MaxWidth,
            Alignment = cacheKey.HorizontalAlignment switch
            {
                0 => TextAlignment.Left,
                1 => TextAlignment.Right,
                2 => TextAlignment.Center,
                _ => throw new ArgumentException("Invalid horizontal alignment")
            },
        };
        block.AddText(text, new Style
            {
                FontFamily = cacheKey.FontName,
                FontSize = cacheKey.FontSize,
                FontWeight = cacheKey.FontWeight,
                FontItalic = cacheKey.FontStyle switch
                {
                    0 => false,
                    1 => true,
                    2 => true, // oblique != italic sometimes, but oh well
                    _ => throw new ArgumentException("Invalid font italic")
                },
                TextColor = cacheKey.Color
            }
        );

        // Vertical alignment. This only changes the position I render the text
        // from, so it doesn't need to be cached.
        float realY = verticalAlignment switch
        {
            // Top-aligned
            0 => y,
            // Bottom-aligned
            1 => bottom - block.MeasuredHeight,
            // Center-aligned
            2 => (y + bottom - block.MeasuredHeight) / 2,
            // Unknown
            _ => throw new ArgumentException("Invalid vertical alignment")
        };

        block.Paint(_skCanvas, new SKPoint(x, realY), new TextPaintOptions
            {
                Edging = _textAntialiasMode switch
                {
                    1 => SKFontEdging.SubpixelAntialias,
                    2 => SKFontEdging.Antialias,
                    3 => SKFontEdging.Alias,
                    _ => SKFontEdging.SubpixelAntialias
                }
            }
        );
    }

    [LibFunction("d2d.get_text_size")]
    private LuaTable D2D_GetTextSize(string text, string fontName, float fontSize, float maximumWidth, float maximumHeight)
    {
        var block = new TextBlock
        {
            MaxWidth = maximumWidth,
            MaxHeight = maximumHeight
        };
        block.AddText(text, new Style
            {
                FontFamily = fontName,
                FontSize = fontSize
            }
        );

        var table = _lua.NewUnnamedTable();
        table["width"] = block.MeasuredWidth;
        table["height"] = block.MeasuredHeight;
        return table;
    }

    [LibFunction("d2d.push_clip")]
    private void D2D_PushClip(float x, float y, float right, float bottom)
    {
        if (_skCanvas == null)
            return;

        _skCanvas.Save();
        _skCanvas.ClipRect(SKRect.Create(x, y, right - x, bottom - y));
    }

    [LibFunction("d2d.pop_clip")]
    private void PopClip()
    {
        _skCanvas?.Restore();
    }

    [LibFunction("d2d.fill_rounded_rectangle")]
    private void D2D_FillRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha)
    {
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        };
        _skCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, paint);
    }

    [LibFunction("d2d.draw_rounded_rectangle")]
    private void D2D_DrawRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha, float thickness)
    {
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        };
        _skCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, paint);
    }

    [LibFunction("d2d.gdip_fillpolygona")]
    private void D2D_GdiPlusFillPolygonA(LuaTable pointsTable, byte alpha, byte red, byte green, byte blue)
    {
        long length = _lua.GetLength(pointsTable);
        var points = new SKPoint[length];

        _lua.IteratePointList(pointsTable, (index, x, y) =>
            {
                points[index - 1] = new SKPoint
                {
                    X = (float) x,
                    Y = (float) y
                };
            }
        );

        using var path = new SKPath();
        path.AddPoly(points);

        using var paint = new SKPaint
        {
            Color = new SKColor(red, green, blue, alpha)
        };

        _skCanvas?.DrawPath(path, paint);
    }


    [LibFunction("d2d.load_image")]
    private void D2D_LoadImage(string path, string identifier)
    {
        if (_imageDict.ContainsKey(identifier))
            throw new InvalidOperationException($"{identifier} already exists");
        var image = SKImage.FromEncodedData(path);
        _imageDict.Add(identifier, image);
    }

    [LibFunction("d2d.free_image")]
    private void D2D_FreeImage(string identifier)
    {
        if (!_imageDict.Remove(identifier, out var image))
            return;

        image.Dispose();
    }

    [LibFunction("d2d.draw_image")]
    private void D2D_DrawImage(float destinationX, float destinationY, float destinationRight, float destinationBottom, float sourceX,
        float sourceY, float sourceRight, float sourceBottom,
        string identifier, float opacity, int interpolation)
    {
        if (!_imageDict.TryGetValue(identifier, out var image))
            throw new ArgumentException("Identifier does not exist");
        using var paint = new SKPaint
        {
            FilterQuality = interpolation == 1 ? SKFilterQuality.Medium : SKFilterQuality.None,
            // multiply alpha by opacity. (@formatter:off)
            ColorFilter = SKColorFilter.CreateColorMatrix(new []
            {
               1.0f,  0.0f,  0.0f,  0.0f,     0.0f, 
               0.0f,  1.0f,  0.0f,  0.0f,     0.0f, 
               0.0f,  0.0f,  1.0f,  0.0f,     0.0f, 
               0.0f,  0.0f,  0.0f,  opacity,  0.0f, 
            })
            // @formatter:on
        };
        _skCanvas?.DrawImage(image,
            source: SKRect.Create(sourceX, sourceY, sourceRight - sourceX, sourceBottom - sourceY),
            dest: SKRect.Create(destinationX, destinationY, destinationRight - destinationX, destinationBottom - destinationY),
            paint
        );
    }

    [LibFunction("d2d.get_image_info")]
    private LuaTable D2D_GetImageInfo(string identifier)
    {
        if (!_imageDict.TryGetValue(identifier, out var image))
            throw new ArgumentException("Identifier does not exist");

        var table = _lua.NewUnnamedTable();
        table["width"] = image.Width;
        table["height"] = image.Height;
        return table;
    }
}