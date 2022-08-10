using System;
using System.Collections.Generic;
using System.Drawing;
using M64RPFW.Gtk.Interfaces;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Gtk.Helpers;

public interface IOpenGLWindow : IDisposable
{
    public static IOpenGLWindow Create(Gdk.Window parent, Size size, Dictionary<GLAttribute, int> attrs)
    {
        Gdk.Display display = parent.Display;
        if (LibGdk.Gdk_IsX11Display(display))
        {
            return new X11OpenGLWindow(parent, size, attrs);
        }

        if (LibGdk.Gdk_IsWaylandDisplay(display))
        {
            return new WlOpenGLWindow(parent, size, attrs);
        }

        throw new InvalidOperationException("This error should not be triggered at all");
    }
    void MakeCurrent();
    void SwapBuffers();
    void SetPosition(Point pos);
    int GetAttribute(GLAttribute attr);
    void ResizeWindow(Size size);
    void SetVisible(bool visible);
    IntPtr GetProcAddress(string symbol);
}