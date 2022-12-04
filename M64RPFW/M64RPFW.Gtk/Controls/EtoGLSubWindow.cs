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

    public Mupen64Plus.Error MakeCurrent()
    {
        try
        {
            Control.MakeCurrent();
            return Mupen64Plus.Error.Success;
        }
        catch (ApplicationException e)
        {
            return Mupen64Plus.Error.Internal;
        }
    }
    public Mupen64Plus.Error SetAttribute(Mupen64Plus.GLAttribute attr, int value)
    {
        return Control.SetAttribute(attr, value);
    }

    public Mupen64Plus.Error GetAttribute(Mupen64Plus.GLAttribute attr, ref int value)
    {
        return Control.GetAttribute(attr, ref value);
    }

    public Mupen64Plus.Error SwapBuffers()
    {
        try
        {
            Control.SwapBuffers();
            return Mupen64Plus.Error.Success;
        }
        catch (ApplicationException e)
        {
            return Mupen64Plus.Error.Internal;
        }
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