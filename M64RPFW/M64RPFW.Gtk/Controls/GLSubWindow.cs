using System;
using System.Collections.Generic;
using System.Drawing;
using Cairo;
using Gtk;
using M64RPFW.Gtk.Helpers;
using M64RPFW.Models.Emulation.Mupen64Plus;
using Point = System.Drawing.Point;
using Rectangle = Gdk.Rectangle;

namespace M64RPFW.Gtk.Controls;

public class GtkGLSubWindow : Widget
{
    public GtkGLSubWindow(Size size)
    {
        _windowSize = size;
        Hexpand = true;
        Vexpand = true;

        _attrMap = new();
    }

    public void SetAttribute(Mupen64Plus.GLAttribute attr, int value)
    {
        if (_glWindow != null)
            throw new InvalidOperationException("Cannot set attributes while window is live");
        _attrMap[attr] = value;
    }

    protected override void OnGetPreferredHeight(out int minimum_height, out int natural_height)
    {
        minimum_height = natural_height = _windowSize.Height;
    }

    protected override void OnGetPreferredWidth(out int minimum_width, out int natural_width)
    {
        minimum_width = natural_width = _windowSize.Width;
    }
    
    protected override void OnSizeAllocated(Rectangle allocation)
    {
        if (_glWindow != null)
        {
            Point pos = new Point
            {
                X = allocation.Left + (allocation.Width - _windowSize.Width) / 2,
                Y = allocation.Top + (allocation.Height - _windowSize.Height) / 2
            };
            _glWindow.SetPosition(pos);
        }

        base.OnSizeAllocated(allocation);
    }

    protected override void OnRealized()
    {
        base.OnRealized();
        _glWindow = IOpenGLWindow.Create(Window, _windowSize, _attrMap);
        Point pos = new Point
        {
            X = Allocation.Left + (Allocation.Width - _windowSize.Width) / 2,
            Y = Allocation.Top + (Allocation.Height - _windowSize.Height) / 2
        };
        _glWindow.SetPosition(pos);
    }

    protected override void OnUnrealized()
    {
        base.OnUnrealized();
        _glWindow!.Dispose();
        _glWindow = null;
    }

    protected override void OnMapped()
    {
        _glWindow!.SetVisible(true);
        base.OnMapped();
    }

    protected override void OnUnmapped()
    {
        _glWindow!.SetVisible(false);
        base.OnUnmapped();
    }

    private Dictionary<Mupen64Plus.GLAttribute, int> _attrMap;
    private Size _windowSize;
    private IOpenGLWindow? _glWindow;
}