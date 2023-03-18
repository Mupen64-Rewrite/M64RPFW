using System;
using System.Collections.Generic;
using System.Drawing;
using M64RPFW.Gtk.Interfaces;
using WaylandSharp;
using M64RPFW.Misc;
using OpenTK.Graphics.Egl;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Gtk.Helpers;

/// <summary>
/// A WL-native subwindow bound to a parent <see cref="Gdk.Window"/>.
/// </summary>
public class WlOpenGLWindow : IOpenGLWindow
{

    public WlOpenGLWindow(Gdk.Window parent, Size size, (int[]? config, int[]? context, int[]? surface) attrs)
    {
        WlGlobals.Init(() => parent.Display.GetWlDisplay());

        _surface = WlGlobals.Compositor.CreateSurface();
        WlSurface parentSurface = parent.GetWlSurface();

        _subsurface = WlGlobals.Subcompositor.GetSubsurface(_surface, parentSurface);
        _subsurface.SetDesync();
        _subsurface.PlaceAbove(parentSurface);

        using (WlRegion opaqueRegion = WlGlobals.Compositor.CreateRegion(),
               inputRegion = WlGlobals.Compositor.CreateRegion())
        {
            opaqueRegion.Add(0, 0, size.Width, size.Height);
            // input region is left empty, since this surface shouldn't take input

            _surface.SetOpaqueRegion(opaqueRegion);
            _surface.SetInputRegion(inputRegion);

            opaqueRegion.Destroy();
            inputRegion.Destroy();
        }
        
        _wlEGLWindow = new WlEglWindow(_surface, size);

        EGLHelpers.InitEGL(
            WlGlobals.Display.RawPointer, _wlEGLWindow.RawPointer, 
            ref _eglDisplay, ref _eglConfig, ref _eglContext, ref _eglSurface, 
            attrs);
        
    }

    public WlOpenGLWindow(Gdk.Window parent, Size size, Dictionary<GLAttribute, int> attrs) : this(parent, size, EGLHelpers.GenEGLAttrs(in attrs))
    {
    }
    public void MakeCurrent()
    {
        if (!Egl.MakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext))
        {
            throw new ApplicationException("EGL error at eglMakeCurrent()");
        }
    }

    public void SwapBuffers()
    {
        if (!Egl.SwapBuffers(_eglDisplay, _eglSurface))
        {
            throw new ApplicationException("EGL error at eglSwapBuffers()");
        }
    }

    public void SetPosition(Point pos)
    {
        _subsurface.SetPosition(pos.X, pos.Y);
        _surface.Commit();
    }

    public int GetAttribute(GLAttribute attr)
    {
        return EGLHelpers.GetConfigAttr(_eglDisplay, _eglConfig, _eglContext, _eglSurface, attr);
    }

    public void ResizeWindow(Size size)
    {
        _wlEGLWindow.Size = size;

        WlRegion opaqueRegion = WlGlobals.Compositor.CreateRegion();
        opaqueRegion.Add(0, 0, size.Width, size.Height);
        _surface.SetOpaqueRegion(opaqueRegion);
        opaqueRegion.Destroy();
    }

    public void SetVisible(bool visible)
    {
        
    }

    public IntPtr GetProcAddress(string symbol)
    {
        return Egl.GetProcAddress(symbol);
    }

    private WlSurface _surface;
    private WlSubsurface _subsurface;
    private WlEglWindow _wlEGLWindow;
    
    private IntPtr _eglDisplay;
    private IntPtr _eglConfig;
    private IntPtr _eglContext;
    private IntPtr _eglSurface;

    private void ReleaseUnmanagedResources()
    {
        Egl.DestroySurface(_eglDisplay, _eglSurface);
        Egl.DestroyContext(_eglDisplay, _eglContext);

        _wlEGLWindow.Dispose();
        _subsurface.Destroy();
        _surface.Destroy();
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _surface.Dispose();
            _subsurface.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~WlOpenGLWindow()
    {
        Dispose(false);
    }
}