using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using M64PRR.Gtk.Interfaces;
using WaylandSharp;

namespace M64PRR.Gtk.Helpers;

/// <summary>
/// A WL-native subwindow bound to a parent <see cref="Gdk.Window"/>.
/// </summary>
public class WlOpenGLWindow : IOpenGLWindow
{
    private static readonly Size TempSize = new Size(640, 480);

    private static readonly int[] EGLConfigAttributes =
    {
        LibEGL.SURFACE_TYPE, LibEGL.WINDOW_BIT,
        LibEGL.RED_SIZE, 8,
        LibEGL.GREEN_SIZE, 8,
        LibEGL.BLUE_SIZE, 8,
        LibEGL.RENDERABLE_TYPE, LibEGL.OPENGL_API,
        LibEGL.NONE
    };

    private static readonly int[] EGLContextAttributes =
    {
        LibEGL.CONTEXT_MAJOR_VERSION_KHR, 3,
        LibEGL.CONTEXT_MINOR_VERSION_KHR, 3,
        LibEGL.NONE
    };

    public WlOpenGLWindow(Gdk.Window parent, int[]? configAttrs = null, int[]? contextAttrs = null)
    {
        WlGlobals.Init(LibGdk.GdkWaylandDisplay_GetWlDisplay(parent.Display));

        _surface = WlGlobals.Compositor.CreateSurface();
        WlSurface parentSurface = LibGdk.GdkWaylandWindow_GetWlSurface(parent);

        _subsurface = WlGlobals.Subcompositor.GetSubsurface(_surface, parentSurface);

        using (WlRegion opaqueRegion = WlGlobals.Compositor.CreateRegion(),
               inputRegion = WlGlobals.Compositor.CreateRegion())
        {
            opaqueRegion.Add(0, 0, TempSize.Width, TempSize.Height);
            // input region is left empty, since this surface shouldn't take input
            
            _surface.SetOpaqueRegion(opaqueRegion);
            _surface.SetInputRegion(inputRegion);

            opaqueRegion.Destroy();
            inputRegion.Destroy();
        }

        InitEGL(configAttrs ?? EGLConfigAttributes, contextAttrs ?? EGLContextAttributes);
    }

    public void MakeCurrent()
    {
        LibEGL.MakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext);
    }

    public void SwapBuffers()
    {
        LibEGL.SwapBuffers(_eglDisplay, _eglSurface);
    }

    public void ResizeWindow(Size size)
    {
        _wlEGLWindow.Size = size;

        WlRegion opaqueRegion = WlGlobals.Compositor.CreateRegion();
        opaqueRegion.Add(0, 0, size.Width, size.Height);
        _surface.SetOpaqueRegion(opaqueRegion);
        opaqueRegion.Destroy();
    }

    private void InitEGL(int[] configAttrs, int[] contextAttrs)
    {
        _eglDisplay = LibEGL.GetDisplay(_surface.RawPointer);
        if (_eglDisplay == IntPtr.Zero)
        {
            throw new ApplicationException("EGL Display creation failed");
        }

        if (!LibEGL.Initialize(_eglDisplay, out var vMajor, out var vMinor))
        {
            throw new ApplicationException("EGL initialization failed");
        }

        Console.WriteLine($"Initialized EGL {vMajor}.{vMinor}");

        // check that KHR_create_context is supported (required for modern OpenGL
        string extensions = Marshal.PtrToStringUTF8(LibEGL.QueryString(_eglDisplay, LibEGL.EXTENSIONS))!;
        if (!extensions.Split(" ").Contains("EGL_KHR_create_context"))
        {
            throw new ApplicationException("Requires support for the KHR_create_context extension");
        }

        LibEGL.GetConfigs(_eglDisplay, null, 0, out var nConfigs);
        if (nConfigs == 0)
        {
            throw new ApplicationException("Could not find any EGL configs");
        }

        IntPtr[] configList = new IntPtr[1];
        LibEGL.ChooseConfig(_eglDisplay, configAttrs, configList, 1, out nConfigs);
        _eglConfig = configList[0];

        _eglContext = LibEGL.CreateContext(_eglDisplay, _eglConfig, IntPtr.Zero, contextAttrs);

        _wlEGLWindow = new WlEGLWindow(_surface, TempSize);
        _eglSurface = LibEGL.CreateWindowSurface(_eglDisplay, _eglConfig, _wlEGLWindow.RawPointer, IntPtr.Zero);
    }

    private WlSurface _surface;
    private WlSubsurface _subsurface;
    private WlEGLWindow _wlEGLWindow;

    private WlEGLWindow _eglWindow;
    private IntPtr _eglDisplay;
    private IntPtr _eglConfig;
    private IntPtr _eglContext;
    private IntPtr _eglSurface;
}