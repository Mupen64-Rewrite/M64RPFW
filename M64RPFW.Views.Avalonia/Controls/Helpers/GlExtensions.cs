using Avalonia;
using Silk.NET.OpenGL;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public static class GlExtensions
{
    public static void Viewport(this GL gl, PixelRect rect)
    {
        gl.Viewport(rect.X, rect.Y, (uint) rect.Width, (uint) rect.Height);
    }

    public static void Viewport(this GL gl, PixelPoint topLeft, PixelSize size)
    {
        gl.Viewport(topLeft.X, topLeft.Y, (uint) size.Width, (uint) size.Height);
    }
}