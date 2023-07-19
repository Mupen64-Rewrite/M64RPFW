using M64RPFW.Services.Abstractions;
using NLua;
using SkiaSharp;
using SkiaExtensions = M64RPFW.Models.Scripting.Extensions.SkiaExtensions;

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    private LuaTable GetWindowSize()
    {
        _lua.NewTable("dimensions");
        var table = _lua.GetTable("dimensions");
        var winSize = _windowSizingService.GetWindowSize();
        table["width"] = (int) winSize.Width;
        table["height"] = (int) winSize.Height;
        return table;
    }

    private void SetWindowSize(int width, int height)
    {
        _windowSizingService.SizeToFit(new WindowSize(width, height), false);
    }
    
    
    private void FillRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha)
    {
        _skCanvas?.DrawRect(x, y, right - x, bottom - y, new SKPaint()
        {
            Color = Extensions.SkiaExtensions.FromFloats(red, green, blue, alpha)
        });
    }

    private void DrawRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, float thickness)
    {
        _skCanvas?.DrawRect(x, y, right - x, bottom - y, new SKPaint()
        {
            Color = Extensions.SkiaExtensions.FromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        });
    }

    private void FillEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha)
    {
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, new SKPaint()
        {
            Color = Extensions.SkiaExtensions.FromFloats(red, green, blue, alpha)
        });
    }

    private void DrawEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha, float thickness)
    {
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, new SKPaint()
        {
            Color = Extensions.SkiaExtensions.FromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        });
    }

    private void DrawLine(float x0, float y0, float x1, float y1, float red, float green, float blue, float alpha,
        float thickness)
    {
        _skCanvas?.DrawLine(x0, y0, x1, y1, new SKPaint()
        {
            Color = Extensions.SkiaExtensions.FromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        });
    }

    private void DrawText(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, string text, string fontName, float fontSize, int fontWeight, int fontStyle,
        int horizontalAlignment, int verticalAlignment, int options)
    {
        // TODO: implement
    }

    private LuaTable GetTextSize(string text, string fontName, float fontSize, float maximumWidth, float maximumHeight)
    {
        // TODO: implement
        _lua.NewTable("text_size");
        var table = _lua.GetTable("text_size");
        table["width"] = 0;
        table["height"] = 0;
        return table;
    }

    private void PushClip(float x, float y, float right, float bottom)
    {
        // TODO: implement
    }

    private void PopClip()
    {
        // TODO: implement
    }

    private void FillRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha)
    {
        _skCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, new SKPaint()
        {
            Color = Extensions.SkiaExtensions.FromFloats(red, green, blue, alpha)
        });
    }

    private void DrawRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha, float thickness)
    {
        _skCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, new SKPaint()
        {
            Color = SkiaExtensions.FromFloats(red, green, blue, alpha),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = thickness
        });
    }

    private void LoadImage(string path, string identifier)
    {
        // TODO: implement
    }

    private void FreeImage()
    {
        // TODO: implement
    }

    private void DrawImage(float sourceX, float sourceY, float sourceRight, float sourceBottom, float destinationX,
        float destinationY, float destinationRight, float destinationBottom,
        string identifier, float opacity, int interpolation)
    {
        // TODO: implement
    }

    private LuaTable GetImageInfo()
    {
        // TODO: implement
        _lua.NewTable("image_info");
        var table = _lua.GetTable("image_info");
        table["width"] = 0;
        table["height"] = 0;
        return table;
    }
    
}