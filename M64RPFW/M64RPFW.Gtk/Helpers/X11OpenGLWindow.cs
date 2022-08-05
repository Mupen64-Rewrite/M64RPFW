using System;
using System.Drawing;

namespace M64RPFW.Gtk.Helpers;

public class X11OpenGLWindow : IOpenGLWindow
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void MakeCurrent()
    {
        throw new NotImplementedException();
    }

    public void SwapBuffers()
    {
        throw new NotImplementedException();
    }

    public void SetPosition(Point pos)
    {
        throw new NotImplementedException();
    }

    public void ResizeWindow(Size size)
    {
        throw new NotImplementedException();
    }

    public void SetVisible(bool visible)
    {
        throw new NotImplementedException();
    }

    public IntPtr GetProcAddress(string symbol)
    {
        throw new NotImplementedException();
    }
}