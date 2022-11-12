using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;
using Windows.Win32.UI.WindowsAndMessaging;
using M64PRR.Wpf.Interfaces;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static Windows.Win32.Graphics.Gdi.SYS_COLOR_INDEX;
using static Windows.Win32.Graphics.OpenGL.PFD_FLAGS;
using static Windows.Win32.Graphics.OpenGL.PFD_PIXEL_TYPE;
using static Windows.Win32.Graphics.OpenGL.PFD_LAYER_TYPE;

namespace M64RPFW.Wpf.Helpers;

public static partial class WGLHelpers
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

                    WGL.LoadBindings(new WGLBindingsContext());
                    fixed (int* outFormatsPtr = outFormats)
                    {
                        if (!WGL.wglChoosePixelFormatARB(
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

        // Class names must be short, this acronym stands for
        // RPFW.(Pixel Format Dummy Window)
        _dummyWindowClassName = new CWStringHolder("RPFW.PFDW");
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