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
using static M64PRR.Wpf.Interfaces.WGLBindings;

namespace M64RPFW.Wpf.Helpers;

public partial class Win32SubWindow
{
    private unsafe LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        switch (uMsg)
        {
            case WM_CREATE:
            {
                IntPtr gcHandlePtr = (IntPtr) ((CREATESTRUCTW*) lParam.Value)->lpCreateParams;

                var inputAttrs = (IDictionary<GLAttribute, int>) GCHandle.FromIntPtr(gcHandlePtr).Target!;
                
                _hDC = GetDC(hWnd);
                if (_hDC == HDC.Null)
                    throw new ApplicationException("Can't get HDC of window");

                
                var attrs = WGLHelpers.ParseAttributes(inputAttrs);
                int pixFormat = WGLHelpers.GetPixelFormat(attrs.pixFmt);

                PIXELFORMATDESCRIPTOR pfd = new();
                DescribePixelFormat(_hDC, PFD_TYPE_RGBA, (uint) sizeof(PIXELFORMATDESCRIPTOR), &pfd);

                if (!SetPixelFormat(_hDC, pixFormat, &pfd))
                    throw new Win32Exception();

                HGLRC tempGLRC = wglCreateContext(_hDC);
                wglMakeCurrent(_hDC, tempGLRC);
                
                WGLBindings.LoadBindings(new WGLBindingsContext());
                
                fixed (int* attrsPtr = attrs.context)
                    _hGLRC = wglCreateContextAttribsARB(_hDC, _hGLRC, attrsPtr);

                wglMakeCurrent(_hDC, HGLRC.Null);
                wglDeleteContext(tempGLRC);
                wglMakeCurrent(_hDC, _hGLRC);
                
                return (LRESULT) 0;
            }
            case WM_DESTROY:
            {
                // Cleanup DC and OpenGL context
                wglMakeCurrent(_hDC, HGLRC.Null);
                wglDeleteContext(_hGLRC);
                ReleaseDC(hWnd, _hDC);
                return (LRESULT) 0;
            }
            default:
                return DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
    }
    
    private HWND _hWnd;
    private HDC _hDC;
    private HGLRC _hGLRC;
}