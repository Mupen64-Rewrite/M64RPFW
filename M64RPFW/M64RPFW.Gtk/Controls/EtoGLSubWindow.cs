using System;
using System.Drawing;
using Eto.Forms;
using Eto.GtkSharp.Forms;
using M64RPFW.Models.Emulation.Core;

namespace M64RPFW.Gtk.Controls;

public class GLSubWindow : GtkControl<GtkGLSubWindow, M64RPFW.Controls.GLSubWindow, Control.ICallback>, M64RPFW.Controls.GLSubWindow.IGLSubWindow
{
    public GLSubWindow()
    {
        Control = new GtkGLSubWindow();
    }

    public Mupen64Plus.Error SetVideoMode(Size size, int bitsPerPixel, Mupen64Plus.VideoMode videoMode, Mupen64Plus.VideoFlags _)
    {
        return Control.SetVideoMode(size, bitsPerPixel, videoMode);
    }

    public void MakeCurrent()
    {
        Control.MakeCurrent();
    }
    public Mupen64Plus.Error SetAttribute(Mupen64Plus.GLAttribute attr, int value)
    {
        return Control.SetAttribute(attr, value);
    }

    public Mupen64Plus.Error SwapBuffers()
    {
        Control.SwapBuffers();
        return Mupen64Plus.Error.Success;
    }

    public Mupen64Plus.Error ResizeWindow(Size size)
    {
        Control.ResizeWindow(size);
        return Mupen64Plus.Error.Success;
    }

    public IntPtr GetProcAddress(string symbol)
    {
        return Control.GetProcAddress(symbol);
    }

    public void CloseVideo()
    {
        Control.CloseVideo();
    }
}