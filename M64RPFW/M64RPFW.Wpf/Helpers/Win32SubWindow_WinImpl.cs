using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;
using Windows.Win32.UI.WindowsAndMessaging;
using M64PRR.Wpf.Interfaces;
using static Windows.Win32.Graphics.OpenGL.PFD_PIXEL_TYPE;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;
using static M64RPFW.Wpf.Helpers.Win32Extras;

namespace M64RPFW.Wpf.Helpers;

public partial class Win32SubWindow
{
    /// <summary>
    /// Function called as the window procedure of the OpenGL window.
    /// Handles setup and cleanup of OpenGL resources.
    /// </summary>
    private static unsafe LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        Win32SubWindow inst;
        if (uMsg == WM_NCCREATE)
        {
            // ((CREATESTRUCTW*) lParam)->lpCreateParams is an array of 2 IntPtrs,
            // the first is the the instance pointer
            IntPtr* gcHandleBlockPtr = (IntPtr*) ((CREATESTRUCTW*) lParam.Value)->lpCreateParams;
            inst = (Win32SubWindow) GCHandle.FromIntPtr(gcHandleBlockPtr[0]).Target!;
            // Save GCHandle pointer to GWLP_USERDATA
            SetWindowLongPtr(hWnd, GWLP_USERDATA, (long) gcHandleBlockPtr[0]);
            return (LRESULT) 1;
        }

        // Retrieve GCHandle pointer from GWLP_USERDATA
        IntPtr gcHandlePtr = (IntPtr) GetWindowLongPtr(hWnd, GWLP_USERDATA);
        GCHandle hnd = GCHandle.FromIntPtr(gcHandlePtr);
        inst = (Win32SubWindow) hnd.Target!;

        if (uMsg == WM_DESTROY)
        {
            // Free the GCHandle, since this is the last time we need it
            hnd.Free();
        }

        switch (uMsg)
        {
            case WM_CREATE:
            {
                IntPtr* gcHandleBlockPtr = (IntPtr*) ((CREATESTRUCTW*) lParam.Value)->lpCreateParams;
                
                var inputAttrs = (IDictionary<GLAttribute, int>) GCHandle.FromIntPtr(gcHandleBlockPtr[1]).Target!;
                
                inst._hDC = GetDC(hWnd);
                if (inst._hDC == HDC.Null)
                    throw new ApplicationException("Can't get HDC of window");
                
                var attrs = WGLHelpers.ParseAttributes(inputAttrs);
                int pixFormat = WGLHelpers.GetPixelFormat(attrs.pixFmt);
                
                // I don't want to pass null to the ppfd parameter of SetPixelFormat
                PIXELFORMATDESCRIPTOR pfd = new();
                if (DescribePixelFormat(inst._hDC, (PFD_PIXEL_TYPE) 1, (uint) sizeof(PIXELFORMATDESCRIPTOR), &pfd) == 0)
                    throw new Win32Exception();

                if (!SetPixelFormat(inst._hDC, pixFormat, &pfd))
                    throw new Win32Exception();
                
                // Create a temp context to call wglCreateContextAttribsARB
                HGLRC tempGLRC = wglCreateContext(inst._hDC);
                if (tempGLRC == HGLRC.Null)
                    throw new Win32Exception();
                if (!wglMakeCurrent(inst._hDC, tempGLRC))
                    throw new Win32Exception();
                
                WGL.LoadBindings(new WGLBindingsContext());
                
                fixed (int* attrsPtr = attrs.context)
                    inst._hGLRC = WGL.wglCreateContextAttribsARB(inst._hDC, inst._hGLRC, attrsPtr);

                if (inst._hGLRC == HGLRC.Null)
                    throw new Win32Exception();
                
                // Activate the *real* context
                if (!wglMakeCurrent(inst._hDC, HGLRC.Null))
                    throw new Win32Exception();
                if (!wglDeleteContext(tempGLRC))
                    throw new Win32Exception();
                if (!wglMakeCurrent(inst._hDC, inst._hGLRC))
                    throw new Win32Exception();

                ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_SHOWNA);
                
                return (LRESULT) 0;
            }
            case WM_DESTROY:
            {
                // Cleanup DC and OpenGL context
                wglMakeCurrent(inst._hDC, HGLRC.Null);
                wglDeleteContext(inst._hGLRC);
                ReleaseDC(hWnd, inst._hDC);
                return (LRESULT) 0;
            }
            case WM_PAINT:
                return (LRESULT) 0;
            default:
                return DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
    }

    // Class names must be short, this acronym stands for
    // RPFW.(openGL Window)
    private static Utf16CString _wndclassName = new("RPFW.GLW");
    private static WNDCLASSW _windowClass = new()
    {
        style = CS_OWNDC,
        lpfnWndProc = WindowProc,
        cbClsExtra = 0,
        cbWndExtra = 0,
        hbrBackground = (HBRUSH) (IntPtr) SYS_COLOR_INDEX.COLOR_BACKGROUND,
        hCursor = HCURSOR.Null,
        hIcon = HICON.Null,
        hInstance = (HINSTANCE) Marshal.GetHINSTANCE(typeof(Win32SubWindow).Module),
        lpszMenuName = null,
        lpszClassName = _wndclassName
    };

    static Win32SubWindow()
    {
        RegisterClass(in _windowClass);
    }
    
    private HWND _hWnd;
    private HDC _hDC;
    private HGLRC _hGLRC;
}