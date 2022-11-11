using System;
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
    public static class WGLHelpers
    {
        private static CWStringHolder _dummyWindowClassName;
        private static WNDCLASSW? _dummyWindowClass;

        private static CWStringHolder _emptyString = new("");

        [ThreadStatic] private static int _pixFormatReturn;
        [ThreadStatic] private static HDC _dummyDC;
        [ThreadStatic] private static HGLRC _dummyGLRC;

        static unsafe WGLHelpers()
        {
            LRESULT DummyWindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
            {
                switch (uMsg)
                {
                    case WM_CREATE:
                    {
                        _dummyDC = GetDC(hWnd);
                        // Setup dummy pixel format
                        PIXELFORMATDESCRIPTOR pfd = new PIXELFORMATDESCRIPTOR
                        {
                            nSize = (ushort) Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
                            nVersion = 1,
                            dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER,
                            iPixelType = PFD_TYPE_RGBA,
                            cColorBits = 32,
                            cRedBits = 0, cRedShift = 0,
                            cGreenBits = 0, cGreenShift = 0,
                            cBlueBits = 0, cBlueShift = 0,
                            cAlphaBits = 0, cAlphaShift = 0,
                            cAccumBits = 0,
                            cAccumRedBits = 0, cAccumGreenBits = 0, cAccumBlueBits = 0, cAccumAlphaBits = 0,
                            cDepthBits = 24,
                            cStencilBits = 8,
                            cAuxBuffers = 0,
                            iLayerType = PFD_MAIN_PLANE,
                            bReserved = 0,
                            dwLayerMask = 0, dwVisibleMask = 0, dwDamageMask = 0
                        };
                        int basePixFmt = ChoosePixelFormat(_dummyDC, &pfd);
                        SetPixelFormat(_dummyDC, basePixFmt, &pfd);

                        // Open WGL context
                        _dummyGLRC = wglCreateContext(_dummyDC);
                        wglMakeCurrent(_dummyDC, _dummyGLRC);
                        
                        // Prepare function I/O
                        int[] outFormats = new int[1];

                        CREATESTRUCTW* initParams = (CREATESTRUCTW*) lParam.Value;
                        int* pixFmtAttribs = (int*) initParams->lpCreateParams;

                        WGLBindings.LoadBindings(new WGLBindingsContext());
                        fixed (int* outFormatsPtr = outFormats)
                        {
                            if (!WGLBindings.wglChoosePixelFormatARB(
                                    _dummyDC, pixFmtAttribs, null,
                                    1, outFormatsPtr,
                                    out var numFormats))
                            {
                                throw new Win32Exception();
                            }
                            
                            if (numFormats < 1)
                                throw new ApplicationException("Could not find WGL pixel format");
                        }
                        _pixFormatReturn = outFormats[0];
                        return (LRESULT) 0;
                    }
                    case WM_DESTROY:
                    {
                        wglMakeCurrent(_dummyDC, (HGLRC) IntPtr.Zero);
                        wglDeleteContext(_dummyGLRC);
                        PostQuitMessage(0);
                        return (LRESULT) 0;
                    }
                    default:
                        return DefWindowProc(hWnd, uMsg, wParam, lParam);
                }
            }
            
            _dummyWindowClassName = new CWStringHolder("M64RPFW.GraphicsDummy");
            _dummyWindowClass = new WNDCLASSW
            {
                style = CS_OWNDC,
                lpfnWndProc = DummyWindowProc,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = (HINSTANCE) Marshal.GetHINSTANCE(typeof(WGLHelpers).Module!),
                hIcon = (HICON) IntPtr.Zero,
                hCursor = (HCURSOR) IntPtr.Zero,
                hbrBackground = (HBRUSH) (nint) (int) COLOR_BACKGROUND,
                lpszMenuName = null,
                lpszClassName = (PCWSTR) _dummyWindowClassName
            };
        }

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

        public static unsafe int GetPixelFormat(int[] pixFmtAttrs)
        {
            fixed (int* pixFmtAttrsPtr = pixFmtAttrs)
            {
                // Create and destroy a dummy window
                // This has the side effect of setting _pixFormatReturn to the selected pixel format
                HWND dummy = CreateWindowEx(
                    0, (PCWSTR) _dummyWindowClassName, (PCWSTR) _emptyString, WS_OVERLAPPED, 
                    CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, 
                    HWND.Null, HMENU.Null, (HINSTANCE) Marshal.GetHINSTANCE(typeof(WGLHelpers).Module), 
                    pixFmtAttrsPtr);
                DestroyWindow(dummy);

                return _pixFormatReturn;
            }
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

        public static explicit operator PCWSTR(CWStringHolder sh)
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