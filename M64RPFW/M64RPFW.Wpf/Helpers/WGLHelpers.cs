using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;
using static Windows.Win32.PInvoke;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;
using System.ComponentModel;
using M64RPFW.Wpf.Interfaces;
using static Windows.Win32.Graphics.OpenGL.PFD_LAYER_TYPE;

namespace M64RPFW.Wpf.Helpers;

internal static partial class WGLHelpers
{
    public static bool IsWGLExtensionSupported(string name)
    {
        return extensionList.Contains(name);
    }
    
    private static int? GetPixelFormatAttributeFor(GLAttribute attr)
    {
        return attr switch
        {
            GLAttribute.DoubleBuffer => WGL.WGL_DOUBLE_BUFFER_ARB,
            GLAttribute.BufferSize => WGL.WGL_COLOR_BITS_ARB,
            GLAttribute.DepthSize => WGL.WGL_DEPTH_BITS_ARB,
            GLAttribute.RedSize => WGL.WGL_RED_BITS_ARB,
            GLAttribute.GreenSize => WGL.WGL_GREEN_BITS_ARB,
            GLAttribute.BlueSize => WGL.WGL_BLUE_BITS_ARB,
            GLAttribute.MultisampleBuffers => WGL.WGL_SAMPLE_BUFFERS_ARB,
            GLAttribute.MultisampleSamples => WGL.WGL_SAMPLES_ARB,
            _ => null
        };
    }

    public static int ChoosePixelFormatM64P(Dictionary<GLAttribute, int> attrs)
    {
        HDC prevDC = wglGetCurrentDC();
        HGLRC prevRC = wglGetCurrentContext();
        try
        {
            if (!wglMakeCurrent(helperDC, helperRC))
                throw new Win32Exception();
            if (!WGL.IsLoaded)
                WGL.LoadFunctions();
            
            int[] pfAttrsArray = (
                from attr in attrs
                let pfAttr = GetPixelFormatAttributeFor(attr.Key)
                where pfAttr != null
                from values in new[] { (int) pfAttr, attr.Value }
                select values
            ).Concat(new[] {WGL.WGL_SUPPORT_OPENGL_ARB, 1, 0}).ToArray();
            int[] results = new int[1];
            if (!WGL.ChoosePixelFormatARB(helperDC, pfAttrsArray, null, results, out var numFormats))
                throw new Win32Exception();
            
            // verify
            int[] check = new int[1];
            WGL.GetPixelFormatAttribs(helperDC, results[0], PFD_MAIN_PLANE, new int[] { WGL.WGL_SUPPORT_OPENGL_ARB },
                check);
            if (check[0] != 1)
            {
                throw new ApplicationException("Context does not support OpenGL?");
            }
            
            if (numFormats == 0)
                throw new InvalidOperationException("OpenGL appears to be unsupported by your graphics driver");

            return results[0];
        }
        finally
        {
            wglMakeCurrent(prevDC, prevRC);
        }
    }

    public static wglDeleteContextSafeHandle CreateContextM64P(SafeHandle dc, Dictionary<GLAttribute, int> attrs)
    {
        HDC prevDC = wglGetCurrentDC();
        HGLRC prevRC = wglGetCurrentContext();
        try
        {
            using SafeHandle tempRC = wglCreateContext(dc);
            wglMakeCurrent(dc, tempRC);
            WGL.LoadFunctions();

            List<int> ctxAttrsList = new();
            if (attrs.TryGetValue(GLAttribute.ContextMajorVersion, out int value))
            {
                ctxAttrsList.AddRange(new[] { WGL.WGL_CONTEXT_MAJOR_VERSION_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.ContextMinorVersion, out value))
            {
                ctxAttrsList.AddRange(new[] { WGL.WGL_CONTEXT_MINOR_VERSION_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.ContextProfileMask, out value))
            {
                int profile = (GLContextType) value switch
                {
                    GLContextType.Core => WGL.WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
                    GLContextType.Compatibilty => WGL.WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB,
                    GLContextType.ES => IsWGLExtensionSupported("WGL_EXT_create_context_es2_profile")
                        ? WGL.WGL_CONTEXT_ES2_PROFILE_BIT_EXT
                        : throw new NotSupportedException(
                            "OpenGL ES2 contexts are not supported by your graphics driver."),
                    _ => throw new ApplicationException("Invalid GLContextType")
                };

                ctxAttrsList.AddRange(new[] { WGL.WGL_CONTEXT_PROFILE_MASK_ARB, profile });
            }

            ctxAttrsList.Add(0);

            int[] ctxAttrsArray = ctxAttrsList.ToArray();
            return WGL.CreateContextAttribsARB(dc, null, ctxAttrsArray);
        }
        finally
        {
            wglMakeCurrent(prevDC, prevRC);
        }
    }
}