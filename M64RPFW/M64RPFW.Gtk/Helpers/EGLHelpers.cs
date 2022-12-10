using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using M64RPFW.Gtk.Interfaces;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Gtk.Helpers;

public static class EGLHelpers
{
    private static readonly int[] DefaultConfigAttributes =
    {
        LibEGL.SURFACE_TYPE, LibEGL.WINDOW_BIT,
        LibEGL.RED_SIZE, 8,
        LibEGL.GREEN_SIZE, 8,
        LibEGL.BLUE_SIZE, 8,
        LibEGL.RENDERABLE_TYPE, LibEGL.OPENGL_API,
        LibEGL.NONE
    };

    private static readonly int[] DefaultContextAttributes =
    {
        LibEGL.CONTEXT_MAJOR_VERSION_KHR, 3,
        LibEGL.CONTEXT_MINOR_VERSION_KHR, 3,
        LibEGL.NONE
    };

    private static readonly int[] DefaultSurfaceAttributes =
    {
        LibEGL.NONE
    };

    public static (int[] config, int[] context, int[] surface) GenEGLAttrs(in Dictionary<GLAttribute, int> attrs)
    {
        List<int> configAttrs = new(), contextAttrs = new(), surfaceAttrs = new();

        if (attrs.TryGetValue(GLAttribute.DoubleBuffer, out int value))
        {
            int eglValue = value != 0 ? LibEGL.BACK_BUFFER : LibEGL.SINGLE_BUFFER;
            surfaceAttrs.AddRange(new[] { LibEGL.RENDER_BUFFER, eglValue });
        }
        else
        {
            surfaceAttrs.AddRange(new[] { LibEGL.RENDER_BUFFER, LibEGL.BACK_BUFFER });
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
                    configAttrs.AddRange(new[]
                    {
                        LibEGL.CONFORMANT, LibEGL.OPENGL_BIT,
                        LibEGL.RENDERABLE_TYPE, LibEGL.OPENGL_BIT
                    });
                    contextAttrs.AddRange(new[]
                    {
                        LibEGL.CONTEXT_OPENGL_PROFILE_MASK_KHR, LibEGL.CONTEXT_OPENGL_COMPATIBILITY_PROFILE_BIT_KHR
                    });
                    break;
                case GLContextType.Core:
                    configAttrs.AddRange(new[]
                    {
                        LibEGL.CONFORMANT, LibEGL.OPENGL_BIT,
                        LibEGL.RENDERABLE_TYPE, LibEGL.OPENGL_BIT
                    });
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

    public static void InitEGL(IntPtr nativeDisplay, IntPtr nativeSurface, ref IntPtr eglDisplay, ref IntPtr eglConfig,
        ref IntPtr eglContext, ref IntPtr eglSurface, (int[]? config, int[]? context, int[]? surface) attrs)
    {
        attrs.config ??= DefaultConfigAttributes;
        attrs.context ??= DefaultContextAttributes;
        attrs.surface ??= DefaultSurfaceAttributes;

        eglDisplay = LibEGL.GetDisplay(nativeDisplay);
        if (eglDisplay == IntPtr.Zero)
        {
            throw new ApplicationException("EGL Display creation failed");
        }

        if (!LibEGL.Initialize(eglDisplay, out var vMajor, out var vMinor))
        {
            throw new ApplicationException("EGL initialization failed");
        }

        LibEGL.BindAPI(RenderApi.GL);

        // This extension is used to bind OpenGL 3.0+ from EGL 1.4 (EGL 1.5 supports this as a core feature)
        string extensions = Marshal.PtrToStringUTF8(LibEGL.QueryString(eglDisplay, LibEGL.EXTENSIONS))!;
        if (!extensions.Split(" ").Contains("EGL_KHR_create_context"))
        {
            throw new ApplicationException("Requires support for the KHR_create_context extension");
        }

        LibEGL.GetConfigs(eglDisplay, null, 0, out var nConfigs);
        if (nConfigs == 0)
        {
            throw new ApplicationException("Could not find any EGL configs");
        }

        IntPtr[] configList = new IntPtr[1];
        LibEGL.ChooseConfig(eglDisplay, attrs.config, configList, 1, out nConfigs);
        eglConfig = configList[0];

        eglContext = LibEGL.CreateContext(eglDisplay, eglConfig, IntPtr.Zero, attrs.context);
        eglSurface = LibEGL.CreateWindowSurface(eglDisplay, eglConfig, nativeSurface, attrs.surface);
    }

    public static int GetConfigAttr(IntPtr display, IntPtr config, IntPtr context, IntPtr surface, GLAttribute attr)
    {
        switch (attr)
        {
            case GLAttribute.DoubleBuffer:
            {
                LibEGL.GetConfigAttrib(display, config, LibEGL.RENDER_BUFFER, out int res);
                switch (res)
                {
                    case LibEGL.BACK_BUFFER:
                        return 1;
                    case LibEGL.SINGLE_BUFFER:
                        return 0;
                }

                break;
            }
            case GLAttribute.BufferSize:
            {
                LibEGL.GetConfigAttrib(display, config, LibEGL.BUFFER_SIZE, out int res);
                return res;
            }
            case GLAttribute.RedSize:
            {
                LibEGL.GetConfigAttrib(display, config, LibEGL.RED_SIZE, out int res);
                return res;
            }
            case GLAttribute.GreenSize:
            {
                LibEGL.GetConfigAttrib(display, config, LibEGL.GREEN_SIZE, out int res);
                return res;
            }
            case GLAttribute.BlueSize:
            {
                LibEGL.GetConfigAttrib(display, config, LibEGL.BLUE_SIZE, out int res);
                return res;
            }
            case GLAttribute.AlphaSize:
            {
                LibEGL.GetConfigAttrib(display, config, LibEGL.ALPHA_SIZE, out int res);
                return res;
            }
            case GLAttribute.SwapControl:
            {
                LibEGL.GetConfigAttrib(display, config, LibEGL.MIN_SWAP_INTERVAL, out int res);
                return res;
            }
            case GLAttribute.MultisampleBuffers:
            {
                LibEGL.GetConfigAttrib(display, config, LibEGL.SAMPLE_BUFFERS, out int res);
                return res;
            }
            case GLAttribute.MultisampleSamples:
            {
                LibEGL.GetConfigAttrib(display, config, LibEGL.SAMPLES, out int res);
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