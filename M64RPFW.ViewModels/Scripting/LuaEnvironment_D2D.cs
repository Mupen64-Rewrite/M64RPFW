using BitFaster.Caching.Lru;
using M64RPFW.ViewModels.Scripting.Extensions;
using M64RPFW.ViewModels.Scripting.Graphics;
using NLua;
using SkiaSharp;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    private const int CACHE_LIMIT = 100;

    private SKCanvas? CurrentCanvas => _renderStack.TryPeek(out var canvas) ? 
        canvas : _skiaSurfaceManagerService?.PrimaryCanvas;

    private int _textAntialiasMode;
    private readonly Dictionary<string, SKImage> _images = new();
    private readonly Dictionary<string, SKSurface> _offscreenSurfaces = new();
    private readonly Stack<SKCanvas> _renderStack = new();

    private readonly ClassicLru<TextPaintParams, SKPaint> _textPaintCache = new(CACHE_LIMIT);
    private readonly ClassicLru<FillPaintParams, SKPaint> _fillPaintCache = new(CACHE_LIMIT);
    private readonly ClassicLru<StrokePaintParams, SKPaint> _strokePaintCache = new(CACHE_LIMIT);
    private readonly ClassicLru<ImagePaintParams, SKPaint> _imagePaintCache = new(CACHE_LIMIT);

    private SKImage? D2DPrivate_FindImage(string key)
    {
        if (_images.TryGetValue(key, out var image))
            return image;
        if (_offscreenSurfaces.TryGetValue(key, out var surface))
            return surface.Snapshot();
        return null;
    }

    [LibFunction("d2d.fill_rectangle")]
    private void D2D_FillRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha)
    {
        var paint = _fillPaintCache.GetOrAdd(
            new FillPaintParams(red, green, blue, alpha),
            PaintFactories.MakeFillPaint
        );
        CurrentCanvas?.DrawRect(x, y, right - x, bottom - y, paint);
    }

    [LibFunction("d2d.draw_rectangle")]
    private void D2D_DrawRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, float thickness)
    {
        var paint = _strokePaintCache.GetOrAdd(
            new StrokePaintParams(thickness, red, green, blue, alpha),
            PaintFactories.MakeStrokePaint
        );
        CurrentCanvas?.DrawRect(x, y, right - x, bottom - y, paint);
    }

    [LibFunction("d2d.fill_ellipse")]
    private void D2D_FillEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha)
    {
        var paint = _fillPaintCache.GetOrAdd(
            new FillPaintParams(red, green, blue, alpha),
            PaintFactories.MakeFillPaint
        );
        CurrentCanvas?.DrawOval(x, y, radiusX, radiusY, paint);
    }

    [LibFunction("d2d.draw_ellipse")]
    private void D2D_DrawEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha, float thickness)
    {
        // FIXME: this needs a workaround, as stroke-only ovals are seemingly not supported?
        // maybe use a path
        var paint = _strokePaintCache.GetOrAdd(
            new StrokePaintParams(thickness, red, green, blue, alpha),
            PaintFactories.MakeStrokePaint
        );
        CurrentCanvas?.DrawRect(x, y, radiusX, radiusY, paint);
    }

    [LibFunction("d2d.draw_line")]
    private void D2D_DrawLine(float x0, float y0, float x1, float y1, float red, float green, float blue, float alpha,
        float thickness)
    {
        var paint = _strokePaintCache.GetOrAdd(
            new StrokePaintParams(thickness, red, green, blue, alpha),
            PaintFactories.MakeStrokePaint
        );
        CurrentCanvas?.DrawLine(x0, y0, x1, y1, paint);
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
        if (CurrentCanvas == null)
            return;
        
        var paint = _textPaintCache.GetOrAdd(new TextPaintParams(
                fontName, fontSize, fontWeight, fontStyle, red, green, blue, alpha
            ), PaintFactories.MakeTextPaint);
        // set antialiasing mode
        (paint.IsAntialias, paint.LcdRenderText) = _textAntialiasMode switch
        {
            0 => (true, true), // default: ClearType
            1 => (true, true), // ClearType (subpixel antialiasing)
            2 => (true, false), // Grayscale (normal antialiasing)
            3 => (false, false), // no antialiasing
            _ => (true, false)
        };
        // subpixel text
        paint.SubpixelText = (options & 0x01) != 0;
        
        var blob = TextLayout.LayoutText(text, paint.ToFont(), right - x, horizontalAlignment switch
            {
                0 => SKTextAlign.Left,
                1 => SKTextAlign.Right,
                2 => SKTextAlign.Center,
                3 => SKTextAlign.Left, // supposed to be "justified", but CBA rn
                _ => SKTextAlign.Left
            }
        );

        var yPos = verticalAlignment switch
        {
            0 => y, // top
            1 => bottom - blob.Bounds.Height, // bottom
            2 => (y + bottom - blob.Bounds.Height) / 2, // center
            _ => y
        };
        var doClip = (options & 0x02) != 0;

        if (doClip)
        {
            CurrentCanvas.Save();
            CurrentCanvas.ClipRect(new SKRect(x, y, right, bottom));
            CurrentCanvas.DrawText(blob, x, yPos, paint);
            CurrentCanvas.Restore();
        }
        else
        {
            CurrentCanvas.DrawText(blob, x, yPos, paint);
        }
    }

    [LibFunction("d2d.get_text_size")]
    private LuaTable D2D_GetTextSize(string text, string fontName, float fontSize, float maximumWidth, float maximumHeight)
    {
        using SKFont font = new SKFont(SKTypeface.FromFamilyName(familyName: fontName), fontSize);
        var blob = TextLayout.LayoutText(text, font, maximumWidth, SKTextAlign.Left);

        var table = _lua.NewUnnamedTable();
        table["width"] = blob?.Bounds.Width ?? 0;
        table["height"] = blob?.Bounds.Height ?? 0;
        return table;
    }

    [LibFunction("d2d.push_clip")]
    private void D2D_PushClip(float x, float y, float right, float bottom)
    {
        if (CurrentCanvas == null)
            return;

        CurrentCanvas.Save();
        CurrentCanvas.ClipRect(SKRect.Create(x, y, right - x, bottom - y));
    }

    [LibFunction("d2d.pop_clip")]
    private void PopClip()
    {
        CurrentCanvas?.Restore();
    }

    [LibFunction("d2d.fill_rounded_rectangle")]
    private void D2D_FillRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha)
    {
        var paint = _fillPaintCache.GetOrAdd(
            new FillPaintParams(red, green, blue, alpha),
            PaintFactories.MakeFillPaint
        );
        CurrentCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, paint);
    }

    [LibFunction("d2d.draw_rounded_rectangle")]
    private void D2D_DrawRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha, float thickness)
    {
        var paint = _strokePaintCache.GetOrAdd(
            new StrokePaintParams(thickness, red, green, blue, alpha),
            PaintFactories.MakeStrokePaint
        );
        CurrentCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, paint);
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
        path.Close();

        var paint = _fillPaintCache.GetOrAdd(
            new FillPaintParams(red, green, blue, alpha),
            PaintFactories.MakeFillPaint
        );

        CurrentCanvas?.DrawPath(path, paint);
    }


    [LibFunction("d2d.load_image")]
    private void D2D_LoadImage(string path, string identifier)
    {
        if (_images.ContainsKey(identifier))
            throw new InvalidOperationException($"{identifier} already exists");
        var image = SKImage.FromEncodedData(path);
        _images.Add(identifier, image);
    }

    [LibFunction("d2d.free_image")]
    private void D2D_FreeImage(string identifier)
    {
        if (!_images.Remove(identifier, out var image))
            return;

        image.Dispose();
    }

    [LibFunction("d2d.draw_image")]
    private void D2D_DrawImage(float destinationX, float destinationY, float destinationRight, float destinationBottom, float sourceX,
        float sourceY, float sourceRight, float sourceBottom,
        string identifier, float opacity, int interpolation)
    {
        SKImage? image;
        if ((image = D2DPrivate_FindImage(identifier)) == null)
            throw new ArgumentException("Identifier does not exist");
        var paint = _imagePaintCache.GetOrAdd(
            new ImagePaintParams(interpolation, opacity),
            PaintFactories.MakeImagePaint
        );
        CurrentCanvas?.DrawImage(image,
            source: SKRect.Create(sourceX, sourceY, sourceRight - sourceX, sourceBottom - sourceY),
            dest: SKRect.Create(destinationX, destinationY, destinationRight - destinationX, destinationBottom - destinationY),
            paint
        );
    }

    [LibFunction("d2d.get_image_info")]
    private LuaTable D2D_GetImageInfo(string identifier)
    {
        SKImage? image;
        if ((image = D2DPrivate_FindImage(identifier)) == null)
            throw new ArgumentException("Identifier does not exist");

        var table = _lua.NewUnnamedTable();
        table["width"] = image.Width;
        table["height"] = image.Height;
        return table;
    }

    [LibFunction("d2d.create_render_target")]
    private string? D2D_CreateRenderTarget(int width, int height)
    {
        var surface = _skiaSurfaceManagerService?.CreateOffscreenBuffer(width, height);
        if (surface == null)
            return null;
        
        string key;
        do
        {
            key = $"__sksurface_{Guid.NewGuid():N}";
        } while (!_offscreenSurfaces.TryAdd(key, surface));

        return key;
    }

    [LibFunction("d2d.destroy_render_target")]
    private void D2D_DestroyRenderTarget(string key)
    {
        if (_offscreenSurfaces.Remove(key, out var surface))
            surface.Dispose();
    }

    [LibFunction("d2d.begin_render_target")]
    private void D2D_BeginRenderTarget(string key)
    {
        if (!_offscreenSurfaces.TryGetValue(key, out var surface))
            return;
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        _renderStack.Push(canvas);
    }

    [LibFunction("d2d.end_render_target")]
    private void D2D_EndRenderTarget(string key)
    {
        if (!_renderStack.TryPop(out var canvas))
            return;
        canvas.Flush();
    }
}