using System;
using System.Collections.Generic;
using System.Drawing;
using M64RPFW.Gtk.Interfaces;
using M64RPFW.Misc;
using M64RPFW.Models.Emulation.Core;

namespace M64RPFW.Gtk.Helpers;

public static class GtkOpenGLWindowFactory
{
    public static IOpenGLWindow Create(Gdk.Window parent, Size size, Dictionary<Mupen64Plus.GLAttribute, int> attrs)
    {
        Gdk.Display display = parent.Display;
        if (display.IsX11Display())
        {
            return new X11OpenGLWindow(parent, size, attrs);
        }

        if (display.IsWaylandDisplay())
        {
            return new WlOpenGLWindow(parent, size, attrs);
        }

        throw new InvalidOperationException("This error should not be triggered at all");
    }
}