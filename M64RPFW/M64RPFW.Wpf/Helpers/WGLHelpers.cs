﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static Windows.Win32.Graphics.Gdi.SYS_COLOR_INDEX;
using static Windows.Win32.Graphics.OpenGL.PFD_FLAGS;
using static Windows.Win32.Graphics.OpenGL.PFD_PIXEL_TYPE;
using static Windows.Win32.Graphics.OpenGL.PFD_LAYER_TYPE;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;
using M64PRR.Wpf.Interfaces;
using OpenTK;
using OpenTK.Graphics.Wgl;
using OpenTK.Platform.Windows;
using static M64RPFW.Wpf.Helpers.WGLConstants;

namespace M64RPFW.Wpf.Helpers
{
    public static partial class WGLHelpers
    {

        public static (int[] pixFmt, int[] context) ParseAttributes(IDictionary<GLAttribute, int> attrs)
        {
            List<int> pixFmt = new()
            {
                WGL_DRAW_TO_WINDOW_ARB, 1,
                WGL_SUPPORT_OPENGL_ARB, 1
            };
            List<int> context = new();

            if (attrs.TryGetValue(GLAttribute.DoubleBuffer, out int value))
            {
                pixFmt.AddRange(new[] { WGL_DOUBLE_BUFFER_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.BufferSize, out value))
            {
                pixFmt.AddRange(new[] { WGL_COLOR_BITS_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.DepthSize, out value))
            {
                pixFmt.AddRange(new[] { WGL_DEPTH_BITS_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.RedSize, out value))
            {
                pixFmt.AddRange(new[] { WGL_RED_BITS_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.GreenSize, out value))
            {
                pixFmt.AddRange(new[] { WGL_GREEN_BITS_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.BlueSize, out value))
            {
                pixFmt.AddRange(new[] { WGL_BLUE_BITS_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.AlphaSize, out value))
            {
                pixFmt.AddRange(new[] { WGL_ALPHA_BITS_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.MultisampleBuffers, out value))
            {
                pixFmt.AddRange(new[] { WGL_SAMPLE_BUFFERS_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.MultisampleSamples, out value))
            {
                pixFmt.AddRange(new[] { WGL_SAMPLES_ARB, value });
            }

            if (attrs.TryGetValue(GLAttribute.ContextMajorVersion, out value))
            {
                context.AddRange(new[] {WGL_CONTEXT_MAJOR_VERSION_ARB, value});
            }

            if (attrs.TryGetValue(GLAttribute.ContextMinorVersion, out value))
            {
                context.AddRange(new[] {WGL_CONTEXT_MAJOR_VERSION_ARB, value});
            }

            if (attrs.TryGetValue(GLAttribute.ContextProfileMask, out value))
            {
                switch ((GLContextType) value)
                {
                    case GLContextType.ES:
                        throw new ArgumentException("OpenGL ES is not supported on Windows");
                    case GLContextType.Compatibilty:
                        context.AddRange(new[]
                        {
                            WGL_CONTEXT_PROFILE_MASK_ARB, 
                            WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB
                        });
                        break;
                    case GLContextType.Core:
                        context.AddRange(new[]
                        {
                            WGL_CONTEXT_PROFILE_MASK_ARB, 
                            WGL_CONTEXT_CORE_PROFILE_BIT_ARB
                        });
                        break;
                }
            }

            return (pixFmt.ToArray(), context.ToArray());
        }
    }

    internal unsafe class CWStringHolder
    {
        public CWStringHolder(string? s)
        {
            _pMem = Marshal.StringToHGlobalUni(s);
        }

        ~CWStringHolder()
        {
            if (_pMem != IntPtr.Zero)
                Marshal.FreeHGlobal(_pMem);
        }

        public static implicit operator PCWSTR(CWStringHolder sh)
        {
            return (char*) sh._pMem;
        }

        private readonly IntPtr _pMem;
    }

    internal class WGLBindingsContext : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return wglGetProcAddress(procName);
        }
    }
}