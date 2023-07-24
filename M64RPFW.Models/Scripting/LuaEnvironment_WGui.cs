using System.Runtime.InteropServices;
using System.Text;
using M64RPFW.Models.Scripting.Extensions;
using M64RPFW.Services.Abstractions;
using NLua;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Topten.RichTextKit;
using SkiaExtensions = M64RPFW.Models.Scripting.Extensions.SkiaExtensions;

// ReSharper disable UnusedMember.Local

namespace M64RPFW.Models.Scripting;

public partial class LuaEnvironment
{
    [LuaFunction("wgui.info")]
    private LuaTable GetWindowSize()
    {
        var table = _lua.NewUnnamedTable();
        var winSize = _frontendScriptingService.WindowAccessService.GetWindowSize();
        table["width"] = (int) winSize.Width;
        table["height"] = (int) winSize.Height;
        return table;
    }

    [LuaFunction("wgui.resize")]
    private void SetWindowSize(int width, int height)
    {
        _frontendScriptingService.WindowAccessService.SizeToFit(new WindowSize(width, height), false);
    }
    
    [LuaFunction("wgui.fill_rectangle")]
    private void FillRectangle(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha)
    {
        using var paint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        };
        _skCanvas?.DrawRect(x, y, right - x, bottom - y, paint);
    }
    
    [LuaFunction("wgui.draw_rectangle")]
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

    [LuaFunction("wgui.fill_ellipse")]
    private void FillEllipse(float x, float y, float radiusX, float radiusY, float red, float green, float blue,
        float alpha)
    {
        using var skPaint = new SKPaint
        {
            Color = SkiaExtensions.ColorFromFloats(red, green, blue, alpha)
        };
        _skCanvas?.DrawOval(x, y, radiusX, radiusY, skPaint);
    }
    
    [LuaFunction("wgui.draw_ellipse")]
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

    [LuaFunction("wgui.draw_line")]
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

    [LuaFunction("wgui.draw_text")]
    private void DrawText(float x, float y, float right, float bottom, float red, float green, float blue,
        float alpha, string text, string fontName, float fontSize, int fontWeight, int fontStyle,
        int horizontalAlignment, int verticalAlignment, int options)
    {
        // TODO: implement
        
        if (_skCanvas == null)
            return;
        
        // RichTextKit handles most layout shenanigans
        var block = new TextBlock
        {
            MaxWidth = right - x,
            MaxHeight = bottom - y,
            Alignment = horizontalAlignment switch
            {
                0 => TextAlignment.Left,
                1 => TextAlignment.Right,
                2 => TextAlignment.Center,
                _ => throw new ArgumentException("Invalid horizontal alignment")
            }
        };
        block.AddText(text, new Style
        {
            FontFamily = fontName,
            FontSize = fontSize,
            FontWeight = fontWeight,
            FontItalic = fontStyle switch
            {
                0 => false,
                1 => true,
                2 => true, // oblique != italic sometimes, but oh well
                _ => throw new ArgumentException("Invalid font italic")
            },
            TextColor = SkiaExtensions.ColorFromFloats(red, green, blue, alpha),
        });
        
        // Vertical alignment
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
        
        block.Paint(_skCanvas, new SKPoint(x, realY), TextPaintOptions.Default);
    }

    [LuaFunction("wgui.get_text_size")]
    private LuaTable GetTextSize(string text, string fontName, float fontSize, float maximumWidth, float maximumHeight)
    {
        // TODO: implement
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

    [LuaFunction("wgui.push_clip")]
    private void PushClip(float x, float y, float right, float bottom)
    {
        if (_skCanvas == null)
            return;
        
        _skCanvas.Save();
        _skCanvas.ClipRect(SKRect.Create(x, y, right - x, bottom - y));
    }

    [LuaFunction("wgui.pop_clip")]
    private void PopClip()
    {
        _skCanvas?.Restore();
    }

    [LuaFunction("wgui.fill_rounded_rectangle")]
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

    [LuaFunction("wgui.draw_rounded_rectangle")]
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

    private Dictionary<string, SKImage> _imageDict;

    [LuaFunction("wgui.load_image")]
    private void LoadImage(string path, string identifier)
    {
        // TODO: implement
        if (_imageDict.ContainsKey(identifier))
            throw new InvalidOperationException($"{identifier} already exists");
        var image = SKImage.FromEncodedData(path);
        _imageDict.Add(identifier, image);
    }

    [LuaFunction("wgui.free_image")]
    private void FreeImage(string identifier)
    {
        // TODO: implement
        if (!_imageDict.Remove(identifier, out var image))
            return;
        
        image.Dispose();
    }

    [LuaFunction("wgui.draw_image")]
    private void DrawImage(float sourceX, float sourceY, float sourceRight, float sourceBottom, float destinationX,
        float destinationY, float destinationRight, float destinationBottom,
        string identifier, float opacity, int interpolation)
    {
        // TODO: implement
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

    [LuaFunction("wgui.get_image_info")]
    private LuaTable GetImageInfo(string identifier)
    {
        // TODO: implement
        if (!_imageDict.TryGetValue(identifier, out var image))
            throw new ArgumentException("Identifier does not exist");

        var table = _lua.NewUnnamedTable();
        table["width"] = image.Width;
        table["height"] = image.Height;
        return table;
    }
    
}