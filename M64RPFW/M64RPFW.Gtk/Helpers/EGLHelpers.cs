using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using M64RPFW.Gtk.Interfaces;
using OpenTK.Graphics.Egl;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Gtk.Helpers;

public static class EGLHelpers
{
    private static readonly int[] DefaultConfigAttributes =
    {
        Egl.SURFACE_TYPE, Egl.WINDOW_BIT,
        Egl.RED_SIZE, 8,
        Egl.GREEN_SIZE, 8,
        Egl.BLUE_SIZE, 8,
        Egl.RENDERABLE_TYPE, Egl.OPENGL_API,
        Egl.NONE
    };

    private static readonly int[] DefaultContextAttributes =
    {
        LibEGL.CONTEXT_MAJOR_VERSION_KHR, 3,
        LibEGL.CONTEXT_MINOR_VERSION_KHR, 3,
        Egl.NONE
    };

    private static readonly int[] DefaultSurfaceAttributes =
    {
        Egl.NONE
    };

    public static (int[] config, int[] context, int[] surface) GenEGLAttrs(in Dictionary<GLAttribute, int> attrs)
    {
        List<int> configAttrs = new(), contextAttrs = new(), surfaceAttrs = new();

        if (attrs.TryGetValue(GLAttribute.DoubleBuffer, out int value))
        {
            int eglValue = value != 0 ? Egl.BACK_BUFFER : Egl.SINGLE_BUFFER;
            surfaceAttrs.AddRange(new[] { Egl.RENDER_BUFFER, eglValue });
        }
        else
        {
            surfaceAttrs.AddRange(new[] { Egl.RENDER_BUFFER, Egl.BACK_BUFFER });
        }

        if (attrs.TryGetValue(GLAttribute.BufferSize, out value))
        {
            configAttrs.AddRange(new[] { Egl.BUFFER_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.DepthSize, out value))
        {
            configAttrs.AddRange(new[] { Egl.DEPTH_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.RedSize, out value))
        {
            configAttrs.AddRange(new[] { Egl.RED_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.GreenSize, out value))
        {
            configAttrs.AddRange(new[] { Egl.GREEN_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.BlueSize, out value))
        {
            configAttrs.AddRange(new[] { Egl.BLUE_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.AlphaSize, out value))
        {
            configAttrs.AddRange(new[] { Egl.ALPHA_SIZE, value });
        }

        if (attrs.TryGetValue(GLAttribute.SwapControl, out value))
        {
            configAttrs.AddRange(new[] { Egl.MIN_SWAP_INTERVAL, value });
        }

        if (attrs.TryGetValue(GLAttribute.MultisampleBuffers, out value))
        {
            configAttrs.AddRange(new[] { Egl.SAMPLE_BUFFERS, value });
        }

        if (attrs.TryGetValue(GLAttribute.MultisampleSamples, out value))
        {
            configAttrs.AddRange(new[] { Egl.SAMPLES, value });
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
                    configAttrs.AddRange(new[]
                    {
                        Egl.CONFORMANT, Egl.OPENGL_BIT,
                        Egl.RENDERABLE_TYPE, Egl.OPENGL_BIT
                    });
                    contextAttrs.AddRange(new[]
                    {
                        LibEGL.CONTEXT_OPENGL_PROFILE_MASK_KHR, LibEGL.CONTEXT_OPENGL_COMPATIBILITY_PROFILE_BIT_KHR
                    });
                    break;
                case GLContextType.Core:
                    configAttrs.AddRange(new[]
                    {
                        Egl.CONFORMANT, Egl.OPENGL_BIT,
                        Egl.RENDERABLE_TYPE, Egl.OPENGL_BIT
                    });
                    contextAttrs.AddRange(new[]
                        { LibEGL.CONTEXT_OPENGL_PROFILE_MASK_KHR, LibEGL.CONTEXT_OPENGL_CORE_PROFILE_BIT_KHR });
                    break;
                case GLContextType.ES:
                    configAttrs.AddRange(new[] { Egl.CONFORMANT, Egl.OPENGL_ES2_BIT });
                    break;
            }
        }

        configAttrs.Add(Egl.NONE);
        contextAttrs.Add(Egl.NONE);
        surfaceAttrs.Add(Egl.NONE);

        return (configAttrs.ToArray(), contextAttrs.ToArray(), surfaceAttrs.ToArray());
    }

    public static unsafe void InitEGL(IntPtr nativeDisplay, IntPtr nativeSurface, ref IntPtr eglDisplay, ref IntPtr eglConfig,
        ref IntPtr eglContext, ref IntPtr eglSurface, (int[]? config, int[]? context, int[]? surface) attrs)
    {
        attrs.config ??= DefaultConfigAttributes;
        attrs.context ??= DefaultContextAttributes;
        attrs.surface ??= DefaultSurfaceAttributes;

        eglDisplay = Egl.GetDisplay(nativeDisplay);
        if (eglDisplay == IntPtr.Zero)
        {
            throw new ApplicationException("EGL Display creation failed");
        }

        if (!Egl.Initialize(eglDisplay, out var vMajor, out var vMinor))
        {
            throw new ApplicationException("EGL initialization failed");
        }

        Egl.BindAPI(RenderApi.GL);

        // This extension is used to bind OpenGL 3.0+ from EGL 1.4 (EGL 1.5 supports this as a core feature)
        string extensions = Marshal.PtrToStringUTF8(Egl.QueryString(eglDisplay, Egl.EXTENSIONS))!;
        if (!extensions.Split(" ").Contains("EGL_KHR_create_context"))
        {
            throw new ApplicationException("Requires support for the KHR_create_context extension");
        }

        Egl.GetConfigs(eglDisplay, null, 0, out var nConfigs);
        if (nConfigs == 0)
        {
            throw new ApplicationException("Could not find any EGL configs");
        }

        IntPtr[] configList = new IntPtr[1];
        Egl.ChooseConfig(eglDisplay, attrs.config, configList, 1, out nConfigs);
        eglConfig = configList[0];

        eglContext = Egl.CreateContext(eglDisplay, eglConfig, IntPtr.Zero, attrs.context);
        fixed (int* pSurfaceAttrs = attrs.surface)
        {
            eglSurface = Egl.CreateWindowSurface(eglDisplay, eglConfig, nativeSurface, (nint) pSurfaceAttrs);
        }
    }

    public static int GetConfigAttr(IntPtr display, IntPtr config, IntPtr context, IntPtr surface, GLAttribute attr)
    {
        switch (attr)
        {
            case GLAttribute.DoubleBuffer:
            {
                Egl.GetConfigAttrib(display, config, Egl.RENDER_BUFFER, out int res);
                switch (res)
                {
                    case Egl.BACK_BUFFER:
                        return 1;
                    case Egl.SINGLE_BUFFER:
                        return 0;
                }

                break;
            }
            case GLAttribute.BufferSize:
            {
                Egl.GetConfigAttrib(display, config, Egl.BUFFER_SIZE, out int res);
                return res;
            }
            case GLAttribute.RedSize:
            {
                Egl.GetConfigAttrib(display, config, Egl.RED_SIZE, out int res);
                return res;
            }
            case GLAttribute.GreenSize:
            {
                Egl.GetConfigAttrib(display, config, Egl.GREEN_SIZE, out int res);
                return res;
            }
            case GLAttribute.BlueSize:
            {
                Egl.GetConfigAttrib(display, config, Egl.BLUE_SIZE, out int res);
                return res;
            }
            case GLAttribute.AlphaSize:
            {
                Egl.GetConfigAttrib(display, config, Egl.ALPHA_SIZE, out int res);
                return res;
            }
            case GLAttribute.SwapControl:
            {
                Egl.GetConfigAttrib(display, config, Egl.MIN_SWAP_INTERVAL, out int res);
                return res;
            }
            case GLAttribute.MultisampleBuffers:
            {
                Egl.GetConfigAttrib(display, config, Egl.SAMPLE_BUFFERS, out int res);
                return res;
            }
            case GLAttribute.MultisampleSamples:
            {
                Egl.GetConfigAttrib(display, config, Egl.SAMPLES, out int res);
                return res;
            }
            case GLAttribute.ContextMajorVersion:
            case GLAttribute.ContextMinorVersion:
            case GLAttribute.ContextProfileMask:
            {
                throw new NotSupportedException("Querying context attributes is unsupported");
            }
        }

        throw new ApplicationException("GetConfigAttr reached end of switch");
    }
}