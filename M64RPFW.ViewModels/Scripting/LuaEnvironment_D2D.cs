using BitFaster.Caching.Lru;
using M64RPFW.ViewModels.Scripting.Extensions;
using M64RPFW.ViewModels.Scripting.Graphics;
using NLua;
using SkiaSharp;
using SkiaExtensions = M64RPFW.ViewModels.Scripting.Extensions.SkiaExtensions;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
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

        using SKFont font = new SKFont(SKTypeface.FromFamilyName(
                familyName: fontName,
                weight: (SKFontStyleWeight) fontWeight,
                width: SKFontStyleWidth.Normal,
                slant: fontStyle switch
                {
                    0 => SKFontStyleSlant.Upright,
                    1 => SKFontStyleSlant.Oblique,
                    2 => SKFontStyleSlant.Italic,
                    _ => throw new ArgumentException($"Invalid style value {fontStyle}")
                }
            ), fontSize
        );
        var blob = TextLayout.LayoutText(text, font, right - x, horizontalAlignment switch
        {
            0 => SKTextAlign.Left,
            1 => SKTextAlign.Right,
            2 => SKTextAlign.Center,
            3 => SKTextAlign.Left, // supposed to be "justified", but CBA rn
            _ => SKTextAlign.Left
        });

        var yPos = verticalAlignment switch
        {
            0 => y, // top
            1 => bottom - blob.Bounds.Height, // bottom
            2 => (y + bottom - blob.Bounds.Height) / 2, // center
        };

        using var paint = new SKPaint(font)
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        };
        
        _skCanvas?.DrawText(blob, x, yPos, paint);
    }

    [LibFunction("d2d.get_text_size")]
    private LuaTable D2D_GetTextSize(string text, string fontName, float fontSize, float maximumWidth, float maximumHeight)
    {
        using SKFont font = new SKFont(SKTypeface.FromFamilyName(familyName: fontName), fontSize);
        var blob = TextLayout.LayoutText(text, font, maximumWidth, SKTextAlign.Left);
        
        var table = _lua.NewUnnamedTable();
        table["width"] = blob.Bounds.Width;
        table["height"] = blob.Bounds.Height;
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