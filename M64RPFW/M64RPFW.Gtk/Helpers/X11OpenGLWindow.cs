using System;
using System.Collections.Generic;
using System.Drawing;
using M64RPFW.Gtk.Interfaces;
using X11;

using static X11.Xlib;
using static M64RPFW.Gtk.Interfaces.LibXlib;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Gtk.Helpers;

public class X11OpenGLWindow : IOpenGLWindow
{

    public X11OpenGLWindow(Gdk.Window parent, Size size, (int[]? config, int[]? context, int[]? surface) attrs)
    {
        var pntWindow = LibGdk.GdkX11Window_GetXID(parent);
        var dpy = LibGdk.GdkX11Display_GetXDisplay(parent.Display);

        var swa = new XSetWindowAttributes
        {
            event_mask = EventMask.ExposureMask
        };

        if (_xDisplay == IntPtr.Zero)
        {
            _xDisplay = dpy;
        }

        // depth = CopyFromParent (0)
        // class = InputOutput (1)
        // visual = CopyFromParent (0)
        // valuemask = CWEventMask (1 << 11) or (0x0800)
        _window = XCreateWindow(dpy, pntWindow, 
            0, 0, (uint) size.Width, (uint) size.Height, 0, 0, 
            1, IntPtr.Zero, 0x0800, ref swa);
        
        // Example I found does this instead of combining
        // the two attributes, I have no idea why
        var xAttrs = new XSetWindowAttributes
        {
            override_redirect = false
        };
        XChangeWindowAttributes(dpy, _window, 0x0200, ref xAttrs);
        
        // Ensure the child is always in front of the parent
        XSetTransientForHint(dpy, _window, pntWindow);
        
        EGLHelpers.InitEGL(
            dpy, (IntPtr) _window, 
            ref _eglDisplay, ref _eglConfig, ref _eglContext, ref _eglSurface, attrs);

        XMapWindow(dpy, _window);
    }
    public X11OpenGLWindow(Gdk.Window parent, Size size, Dictionary<GLAttribute, int> attrs) : this(parent, size, EGLHelpers.GenEGLAttrs(in attrs))
    {
    }

    public void MakeCurrent()
    {
        LibEGL.MakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext);
    }

    public void SwapBuffers()
    {
        LibEGL.SwapBuffers(_eglDisplay, _eglSurface);
    }

    public void SetPosition(Point pos)
    {
        XMoveWindow(_xDisplay, _window, pos.X, pos.Y);
    }

    public int GetAttribute(GLAttribute attr)
    {
        return EGLHelpers.GetConfigAttr(_eglDisplay, _eglConfig, _eglContext, _eglSurface, attr);
    }

    public void ResizeWindow(Size size)
    {
        XResizeWindow(_xDisplay, _window, (uint) size.Width, (uint) size.Height);
    }

    public void SetVisible(bool visible)
    {
        if (visible)
            XMapWindow(_xDisplay, _window);
        else
            XUnmapWindow(_xDisplay, _window);
    }

    public IntPtr GetProcAddress(string symbol)
    {
        return LibEGL.GetProcAddress(symbol);
    }

    private static IntPtr _xDisplay = IntPtr.Zero;

    private Window _window;
    
    private IntPtr _eglDisplay;
    private IntPtr _eglConfig;
    private IntPtr _eglContext;
    private IntPtr _eglSurface;

    private void ReleaseUnmanagedResources()
    {
        XUnmapWindow(_xDisplay, _window);
        XDestroyWindow(_xDisplay, _window);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~X11OpenGLWindow()
    {
        ReleaseUnmanagedResources();
    }
}