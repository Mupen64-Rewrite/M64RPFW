using M64RPFW.Models.Scripting.Extensions;
using M64RPFW.Services.Abstractions;
using NLua;
using SkiaSharp;
using BitFaster.Caching.Lru;
using M64RPFW.Models.Scripting.Graphics;
using Topten.RichTextKit;
using SkiaExtensions = M64RPFW.Models.Scripting.Extensions.SkiaExtensions;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    private const int TextLayoutCacheLimit = 1000;
    
    private readonly ClassicLru<TextLayoutParameters, TextBlock> _textLayoutCache = new(TextLayoutCacheLimit);
    private int _textAntialiasMode;
    private readonly Dictionary<string, SKImage> _imageDict = new();
    
    [LuaFunction("d2d.fill_rectangle")]
    private void FillRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha)
    {
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        };
        _skCanvas?.DrawRect(x, y, right - x, bottom - y, paint);
    }

    [LuaFunction("d2d.draw_rectangle")]
    private void DrawRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
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

    [LuaFunction("d2d.fill_ellipse")]
    private void FillEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha)
    {
        using var skPaint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        };
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, skPaint);
    }

    [LuaFunction("d2d.draw_ellipse")]
    private void DrawEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha, float thickness)
    {
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        };
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, paint);
    }

    [LuaFunction("d2d.draw_line")]
    private void DrawLine(float x0, float y0, float x1, float y1, float red, float green, float blue, float alpha,
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


    [LuaFunction("d2d.set_text_antialias_mode")]
    private void SetTextAntialiasMode(int mode)
    {
        _textAntialiasMode = mode;
    }

    [LuaFunction("d2d.draw_text")]
    private void DrawText(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, string text, string fontName, float fontSize, int fontWeight, int fontStyle,
        int horizontalAlignment, int verticalAlignment, int options)
    {
        if (_skCanvas == null)
            return;
        
        // Cache laid-out text blocks. Unfortunately, I have to key with
        // colour, though I wish I didn't need to.
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
        var block = _textLayoutCache.GetOrAdd(cacheKey, key =>
        {
            var block = new TextBlock
            {
                MaxWidth = key.MaxWidth,
                MaxHeight = key.MaxHeight,
                Alignment = key.HorizontalAlignment switch
                {
                    0 => TextAlignment.Left,
                    1 => TextAlignment.Right,
                    2 => TextAlignment.Center,
                    _ => throw new ArgumentException("Invalid horizontal alignment")
                },
            };
            block.AddText(text, new Style
            {
                FontFamily = key.FontName,
                FontSize = key.FontSize,
                FontWeight = key.FontWeight,
                FontItalic = key.FontStyle switch
                {
                    0 => false,
                    1 => true,
                    2 => true, // oblique != italic sometimes, but oh well
                    _ => throw new ArgumentException("Invalid font italic")
                },
                TextColor = key.Color
            });
            return block;
        });
        
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
        });
    }

    [LuaFunction("d2d.get_text_size")]
    private LuaTable GetTextSize(string text, string fontName, float fontSize, float maximumWidth, float maximumHeight)
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
        });

        var table = _lua.NewUnnamedTable();
        table["width"] = block.MeasuredWidth;
        table["height"] = block.MeasuredHeight;
        return table;
    }

    [LuaFunction("d2d.push_clip")]
    private void PushClip(float x, float y, float right, float bottom)
    {
        if (_skCanvas == null)
            return;

        _skCanvas.Save();
        _skCanvas.ClipRect(SKRect.Create(x, y, right - x, bottom - y));
    }

    [LuaFunction("d2d.pop_clip")]
    private void PopClip()
    {
        _skCanvas?.Restore();
    }

    [LuaFunction("d2d.fill_rounded_rectangle")]
    private void FillRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha)
    {
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        };
        _skCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, paint);
    }

    [LuaFunction("d2d.draw_rounded_rectangle")]
    private void DrawRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
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

    [LuaFunction("d2d.gdip_fillpolygona")]
    private void GdiPlusFillPolygonA(LuaTable pointsTable, byte alpha, byte red, byte green, byte blue)
    {
        // NLua API isn't good enough: we need KeraLua
        KeraLua.Lua state = _lua.State!;
        _lua.Push(pointsTable);

        // Utility functions (that restore the stack)
        long GetLength()
        {
            state.PushLength(-1);
            var res = state.ToInteger(-1);
            state.Pop(1);
            return res;
        }

        SKPoint GetPoint(long key)
        {
            // stack = [table, table[key]]
            state.PushInteger(key);
            state.GetTable(-2);

            // stack = [table, table[key], table[key][1], table[key][2]]
            state.GetInteger(-1, 1);
            state.GetInteger(-2, 2);

            var res = new SKPoint((float) state.ToNumber(-2), (float) state.ToNumber(-1));

            state.Pop(3);
            return res;
        }

        var points = new List<SKPoint>();

        long len = GetLength();
        for (long i = 1; i <= len; i++)
        {
            points.Add(GetPoint(i));
        }
        // All local functions restore the stack, so we need only pop the table 
        state.Pop(1);

        using var path = new SKPath();
        path.AddPoly(points.ToArray());

        using var paint = new SKPaint
        {
            Color = new SKColor(red, green, blue, alpha)
        };

        _skCanvas?.DrawPath(path, paint);
    }


    [LuaFunction("d2d.load_image")]
    private void LoadImage(string path, string identifier)
    {
        if (_imageDict.ContainsKey(identifier))
            throw new InvalidOperationException($"{identifier} already exists");
        var image = SKImage.FromEncodedData(path);
        _imageDict.Add(identifier, image);
    }

    [LuaFunction("d2d.free_image")]
    private void FreeImage(string identifier)
    {
        if (!_imageDict.Remove(identifier, out var image))
            return;

        image.Dispose();
    }

    [LuaFunction("d2d.draw_image")]
    private void DrawImage(float sourceX, float sourceY, float sourceRight, float sourceBottom, float destinationX,
        float destinationY, float destinationRight, float destinationBottom,
        string identifier, float opacity, int interpolation)
    {
        if (!_imageDict.TryGetValue(identifier, out var image))
            throw new ArgumentException("Identifier does not exist");

        // This complicated setup simply multiplies the alpha component by `opacity`.
        // ==========================================================================
        using var paint = new SKPaint
        {
            FilterQuality = interpolation == 1 ? SKFilterQuality.Medium : SKFilterQuality.None,
            MaskFilter = SKMaskFilter.CreateClip((byte) (opacity * byte.MaxValue), (byte) (opacity * byte.MaxValue))
        };
        _skCanvas?.DrawImage(image,
            source: SKRect.Create(sourceX, sourceY, sourceRight - sourceX, sourceBottom - sourceY),
            dest: SKRect.Create(destinationX, destinationY, destinationRight - destinationX, destinationBottom - destinationY),
            paint);
    }

    [LuaFunction("d2d.get_image_info")]
    private LuaTable GetImageInfo(string identifier)
    {
        if (!_imageDict.TryGetValue(identifier, out var image))
            throw new ArgumentException("Identifier does not exist");

        var table = _lua.NewUnnamedTable();
        table["width"] = image.Width;
        table["height"] = image.Height;
        return table;
    }

}