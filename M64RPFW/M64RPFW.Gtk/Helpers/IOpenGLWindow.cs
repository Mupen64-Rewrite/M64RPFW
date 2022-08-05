using System;
using System.Collections.Generic;
using System.Drawing;
using M64PRR.Gtk.Helpers;
using M64RPFW.Gtk.Interfaces;
using static M64RPFW.Models.Emulation.Mupen64Plus.Mupen64Plus;

namespace M64RPFW.Gtk.Helpers;

public interface IOpenGLWindow : IDisposable
{
    public static IOpenGLWindow Create(Gdk.Window parent, Size size, Dictionary<GLAttribute, int> attrs)
    {
        Gdk.Display display = parent.Display;
        if (LibGdk.Gdk_IsX11Display(display))
        {
            // TODO implement X11 windows
            throw new NotImplementedException("X11 windows are not implemented yet");
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
    void ResizeWindow(Size size);
    void SetVisible(bool visible);
    IntPtr GetProcAddress(string symbol);
}