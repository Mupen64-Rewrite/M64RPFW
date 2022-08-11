using System;
using System.Collections.Generic;
using System.Drawing;
using Gtk;
using M64RPFW.Gtk.Helpers;
using Point = System.Drawing.Point;
using Rectangle = Gdk.Rectangle;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Gtk.Controls;

public class GtkGLSubWindow : Widget
{
    public GtkGLSubWindow() : this(new Size { Width = 640, Height = 480 })
    {
    }

    public GtkGLSubWindow(Size size)
    {
        Hexpand = true;
        Vexpand = true;

        _windowSize = size;
        _attrMap = new();
        _queueRealize = false;

        HasWindow = false;
    }

    public Error SetAttribute(GLAttribute attr, int value)
    {
        if (_glWindow != null)
            return Error.InvalidState;
        _attrMap[attr] = value;
        return Error.Success;
    }

    public Error GetAttribute(GLAttribute attr, ref int value)
    {
        if (_glWindow == null)
        {
            return Error.InvalidState;
        }

        try
        {
            value = _glWindow.GetAttribute(attr);
            return Error.Success;
        }
        catch (NotSupportedException)
        {
            return Error.Unsupported;
        }
        catch (Exception)
        {
            return Error.Internal;
        }
    }

    public Error SetVideoMode(Size size, int bitsPerPixel, VideoMode screenMode)
    {
        if (screenMode != VideoMode.Windowed)
        {
            return Error.Unsupported;
        }

        _windowSize = size;

        InitGLWindow();

        return Error.Success;
    }

    public void MakeCurrent()
    {
        _glWindow?.MakeCurrent();
    }

    public void SwapBuffers()
    {
        _glWindow?.SwapBuffers();
    }

    public void ResizeWindow(Size size)
    {
        _windowSize = size;
        SetSizeRequest(size.Width, size.Height);
    }

    public IntPtr GetProcAddress(string symbol)
    {
        return _glWindow!.GetProcAddress(symbol);
    }

    public void CloseVideo()
    {
        _glWindow?.Dispose();
        _glWindow = null;
    }

    private void InitGLWindow()
    {
        _glWindow = IOpenGLWindow.Create(Window, _windowSize, _attrMap);
        Console.WriteLine($"Allocation top-left: ({Allocation.X}, {Allocation.Y})");

        int basePosX = Allocation.Left + (Allocation.Width - _windowSize.Width) / 2,
            basePosY = Allocation.Top + (Allocation.Height - _windowSize.Height) / 2;

        TranslateCoordinates(Toplevel, basePosX, basePosY, out int absX, out int absY);
        _glWindow.SetPosition(new Point(basePosX, basePosY));
    }

    #region GTK virtual methods

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
            Console.WriteLine($"Allocation rect: {allocation}");
            Point pos = new Point
            {
                X = allocation.X + (allocation.Width - _windowSize.Width) / 2,
                Y = allocation.Y + (allocation.Height - _windowSize.Height) / 2
            };
            _glWindow.SetPosition(pos);
        }

        base.OnSizeAllocated(allocation);
    }

    protected override void OnRealized()
    {
        base.OnRealized();
        if (_queueRealize)
            InitGLWindow();
    }

    protected override void OnUnrealized()
    {
        base.OnUnrealized();
        CloseVideo();
    }

    protected override void OnMapped()
    {
        _glWindow?.SetVisible(true);
        base.OnMapped();
    }

    protected override void OnUnmapped()
    {
        _glWindow?.SetVisible(false);
        base.OnUnmapped();
    }

    #endregion

    private Dictionary<GLAttribute, int> _attrMap;
    private Size _windowSize;
    private bool _queueRealize;
    private IOpenGLWindow? _glWindow;
}