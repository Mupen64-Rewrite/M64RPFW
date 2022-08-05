using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using M64RPFW.Gtk.Helpers;
using M64RPFW.Gtk.Interfaces;
using M64RPFW.Models.Emulation.Mupen64Plus;
using WaylandSharp;
using static M64RPFW.Models.Emulation.Mupen64Plus.Mupen64Plus;

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

    private static readonly int[] EGLSurfaceAttributes = null;

    public WlOpenGLWindow(Gdk.Window parent, Size size, (int[]? config, int[]? context, int[]? surface) attrs)
    {
        WlGlobals.Init(LibGdk.GdkWaylandDisplay_GetWlDisplay(parent.Display));

        _surface = WlGlobals.Compositor.CreateSurface();
        WlSurface parentSurface = LibGdk.GdkWaylandWindow_GetWlSurface(parent);

        _subsurface = WlGlobals.Subcompositor.GetSubsurface(_surface, parentSurface);

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

        InitEGL(
            size, attrs.config ?? EGLConfigAttributes, 
            attrs.context ?? EGLContextAttributes,
            attrs.surface ?? EGLSurfaceAttributes);
    }

    public WlOpenGLWindow(Gdk.Window parent, Size size, Dictionary<GLAttribute, int> attrs) : this(parent, size, GenEGLAttrs(in attrs))
    {
    }

    private static (int[] config, int[] context, int[] surface) GenEGLAttrs(in Dictionary<GLAttribute, int> attrs)
    {
        List<int> configAttrs = new(), contextAttrs = new(), surfaceAttrs = new();

        if (attrs.TryGetValue(GLAttribute.DoubleBuffer, out int value))
        {
            int eglValue = value != 0 ? LibEGL.BACK_BUFFER : LibEGL.SINGLE_BUFFER;
            surfaceAttrs.AddRange(new[] { LibEGL.RENDER_BUFFER, eglValue });
        }

        if (attrs.TryGetValue(GLAttribute.BufferSize, out value))
        {
            configAttrs.AddRange(new[] { LibEGL.BUFFER_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.DepthSize, out value))
        {
            configAttrs.AddRange(new[] { LibEGL.DEPTH_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.RedSize, out value))
        {
            configAttrs.AddRange(new[] { LibEGL.RED_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.GreenSize, out value))
        {
            configAttrs.AddRange(new[] { LibEGL.GREEN_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.BlueSize, out value))
        {
            configAttrs.AddRange(new[] { LibEGL.BLUE_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.AlphaSize, out value))
        {
            configAttrs.AddRange(new[] { LibEGL.ALPHA_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.SwapControl, out value))
        {
            configAttrs.AddRange(new[] { LibEGL.MIN_SWAP_INTERVAL, value });
        }

        if (attrs.TryGetValue(GLAttribute.MultisampleBuffers, out value))
        {
            configAttrs.AddRange(new[] { LibEGL.SAMPLE_BUFFERS, value });
        }

        if (attrs.TryGetValue(GLAttribute.MultisampleSamples, out value))
        {
            configAttrs.AddRange(new[] { LibEGL.SAMPLES, value });
        }

        if (attrs.TryGetValue(GLAttribute.ContextMajorVersion, out value))
        {
            contextAttrs.AddRange(new[] { LibEGL.CONTEXT_MAJOR_VERSION_KHR, value });
        }

        if (attrs.TryGetValue(GLAttribute.ContextMinorVersion, out value))
        {
            contextAttrs.AddRange(new[] { LibEGL.CONTEXT_MINOR_VERSION_KHR, value });
        }

        if (attrs.TryGetValue(GLAttribute.ContextProfileMask, out value))
        {
            switch ((GLContextType) value)
            {
                case GLContextType.Compatibilty:
                    configAttrs.AddRange(new[] { LibEGL.CONFORMANT, LibEGL.OPENGL_BIT });
                    contextAttrs.AddRange(new[]
                    {
                        LibEGL.CONTEXT_OPENGL_PROFILE_MASK_KHR, LibEGL.CONTEXT_OPENGL_COMPATIBILITY_PROFILE_BIT_KHR
                    });
                    break;
                case GLContextType.Core:
                    configAttrs.AddRange(new[] { LibEGL.CONFORMANT, LibEGL.OPENGL_BIT });
                    contextAttrs.AddRange(new[]
                        { LibEGL.CONTEXT_OPENGL_PROFILE_MASK_KHR, LibEGL.CONTEXT_OPENGL_CORE_PROFILE_BIT_KHR });
                    break;
                case GLContextType.ES:
                    configAttrs.AddRange(new[] { LibEGL.CONFORMANT, LibEGL.OPENGL_ES2_BIT });
                    break;
            }
        }

        configAttrs.Add(LibEGL.NONE);
        contextAttrs.Add(LibEGL.NONE);
        surfaceAttrs.Add(LibEGL.NONE);

        return (configAttrs.ToArray(), contextAttrs.ToArray(), surfaceAttrs.ToArray());
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
        _subsurface.SetPosition(pos.X, pos.Y);
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

    private void InitEGL(Size size, int[] configAttrs, int[] contextAttrs, int[] surfaceAttrs)
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

        _wlEGLWindow = new WlEGLWindow(_surface, size);
        _eglSurface = LibEGL.CreateWindowSurface(_eglDisplay, _eglConfig, _wlEGLWindow.RawPointer, surfaceAttrs);
    }

    private WlSurface _surface;
    private WlSubsurface _subsurface;
    private WlEGLWindow _wlEGLWindow;

    private WlEGLWindow _eglWindow;
    private IntPtr _eglDisplay;
    private IntPtr _eglConfig;
    private IntPtr _eglContext;
    private IntPtr _eglSurface;

    private void ReleaseUnmanagedResources()
    {
        LibEGL.DestroySurface(_eglDisplay, _eglSurface);
        LibEGL.DestroyContext(_eglDisplay, _eglContext);

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
            _eglWindow.Dispose();
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