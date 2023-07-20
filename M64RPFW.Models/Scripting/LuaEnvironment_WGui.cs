using M64RPFW.Services.Abstractions;
using NLua;
using SkiaSharp;
using SkiaExtensions = M64RPFW.Models.Scripting.Extensions.SkiaExtensions;

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    [LuaFunction("wgui.info")]
    private LuaTable GetWindowSize()
    {
        _lua.NewTable("__2dimensions");
        var table = _lua.GetTable("__2dimensions");
        var winSize = _frontendScriptingService.WindowSizingService.GetWindowSize();
        table["width"] = (int) winSize.Width;
        table["height"] = (int) winSize.Height;
        return table;
    }

    [LuaFunction("wgui.resize")]
    private void SetWindowSize(int width, int height)
    {
        _frontendScriptingService.WindowSizingService.SizeToFit(new WindowSize(width, height), false);
    }
    
    [LuaFunction("wgui.fill_rectangle")]
    private void FillRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha)
    {
        _skCanvas?.DrawRect(x, y, right - x, bottom - y, new SKPaint()
        {
            Color = Extensions.SkiaExtensions.FromFloats(red, green, blue, alpha)
        });
    }
    
    [LuaFunction("wgui.draw_rectangle")]
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

    [LuaFunction("wgui.fill_ellipse")]
    private void FillEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha)
    {
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, new SKPaint()
        {
            Color = Extensions.SkiaExtensions.FromFloats(red, green, blue, alpha)
        });
    }
    
    [LuaFunction("wgui.draw_ellipse")]
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

    [LuaFunction("wgui.draw_line")]
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

    [LuaFunction("wgui.draw_text")]
    private void DrawText(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, string text, string fontName, float fontSize, int fontWeight, int fontStyle,
        int horizontalAlignment, int verticalAlignment, int options)
    {
        // TODO: implement
    }

    [LuaFunction("wgui.get_text_size")]
    private LuaTable GetTextSize(string text, string fontName, float fontSize, float maximumWidth, float maximumHeight)
    {
        // TODO: implement
        _lua.NewTable("___text_size");
        var table = _lua.GetTable("___text_size");
        table["width"] = 0;
        table["height"] = 0;
        return table;
    }

    [LuaFunction("wgui.push_clip")]
    private void PushClip(float x, float y, float right, float bottom)
    {
        // TODO: implement
    }

    [LuaFunction("wgui.pop_clip")]
    private void PopClip()
    {
        // TODO: implement
    }

    [LuaFunction("wgui.fill_rounded_rectangle")]
    private void FillRoundedRectangle(float x, float y, float right, float bottom, float radiusX, float radiusY,
        float red, float green, float blue,
        float alpha)
    {
        _skCanvas?.DrawRoundRect(x, y, right - x, bottom - y, radiusX, radiusY, new SKPaint()
        {
            Color = Extensions.SkiaExtensions.FromFloats(red, green, blue, alpha)
        });
    }

    [LuaFunction("wgui.draw_rounded_rectangle")]
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

    [LuaFunction("wgui.load_image")]
    private void LoadImage(string path, string identifier)
    {
        // TODO: implement
    }

    [LuaFunction("wgui.free_image")]
    private void FreeImage()
    {
        // TODO: implement
    }

    [LuaFunction("wgui.draw_image")]
    private void DrawImage(float sourceX, float sourceY, float sourceRight, float sourceBottom, float destinationX,
        float destinationY, float destinationRight, float destinationBottom,
        string identifier, float opacity, int interpolation)
    {
        // TODO: implement
    }

    [LuaFunction("wgui.get_image_info")]
    private LuaTable GetImageInfo()
    {
        // TODO: implement
        _lua.NewTable("___image_info");
        var table = _lua.GetTable("___image_info");
        table["width"] = 0;
        table["height"] = 0;
        return table;
    }
    
}