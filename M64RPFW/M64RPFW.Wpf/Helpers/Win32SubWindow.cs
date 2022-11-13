using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Windows.Win32;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using M64PRR.Wpf.Interfaces;
using OpenTK.Graphics.OpenGL4;
using static Windows.Win32.Graphics.OpenGL.PFD_LAYER_TYPE;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.PEEK_MESSAGE_REMOVE_TYPE;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static M64RPFW.Wpf.Helpers.WGLConstants;

namespace M64RPFW.Wpf.Helpers;

public partial class Win32SubWindow : IDisposable
{
    private static Utf16CString _glWindowTitle = new("M64RPFW OpenGL output");
    public unsafe Win32SubWindow(System.Windows.Window parent, Size size, IDictionary<GLAttribute, int> attrs)
    {
        GCHandle attrsHandle = GCHandle.Alloc(attrs);
        GCHandle thisHandle = GCHandle.Alloc(this);

        // We need to pass both GCHandles to WM_CREATE, so we create a block of
        // unmanaged memory to store them both
        IntPtr memBlock = Marshal.AllocHGlobal(sizeof(IntPtr) * 2);
        ((IntPtr*) memBlock)[0] = GCHandle.ToIntPtr(thisHandle);
        ((IntPtr*) memBlock)[1] = GCHandle.ToIntPtr(attrsHandle);
        
        // Get parent HWND
        HWND parentHwnd = (HWND) new WindowInteropHelper(parent).Handle;
        
        try
        {
            // Create child window that clips other windows and does not receive input
            _hWnd = CreateWindowEx(0, _wndclassName, _glWindowTitle,
                WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS | WS_DISABLED,
                CW_USEDEFAULT, CW_USEDEFAULT, size.Width, size.Height,
                parentHwnd, HMENU.Null, (HINSTANCE) Marshal.GetHINSTANCE(typeof(Win32SubWindow).Module),
                (void*) memBlock);
            if (_hWnd == HWND.Null)
                throw new Win32Exception();
        }
        finally
        {
            attrsHandle.Free();
            Marshal.FreeHGlobal(memBlock);
        }
    }

    public void MakeCurrent()
    {
        if (!wglMakeCurrent(_hDC, _hGLRC))
        {
            int err = Marshal.GetLastWin32Error();
            if (err is not 170)
                throw new Win32Exception(err);
        }
    }

    public void SwapBuffers()
    {
        if (!PInvoke.SwapBuffers(_hDC))
            throw new Win32Exception();
        // Handle any incoming messages
        while (PeekMessage(out var msg, _hWnd, 0, 0, PM_REMOVE))
        {
            TranslateMessage(in msg);
            DispatchMessage(in msg);
        }
    }

    public void Dispose()
    {
        DestroyWindow(_hWnd);
    }

    public void SetPosition(Point pos)
    {
        if (!GetWindowRect(_hWnd, out var winRect))
            throw new Win32Exception();

        MoveWindow(_hWnd, pos.X, pos.Y, winRect.Width, winRect.Height, false);
    }

    public int GetAttribute(GLAttribute attr)
    {
        int pixFmt = GetPixelFormat(_hDC);
        MakeCurrent();
        
        WGL.LoadBindings(new WGLBindingsContext());
        GL.LoadBindings(new WGLBindingsContext());
        
        switch (attr)
        {
            case GLAttribute.DoubleBuffer:
            {
                var outValues = new int[1];
                if (!WGL.wglGetPixelFormatAttribivARB(_hDC, pixFmt, PFD_MAIN_PLANE,
                        new[] { WGL_DOUBLE_BUFFER_ARB }, outValues))
                    throw new Win32Exception();

                return outValues[0];
            }
            case GLAttribute.BufferSize:
            {
                var outValues = new int[1];
                if (!WGL.wglGetPixelFormatAttribivARB(_hDC, pixFmt, PFD_MAIN_PLANE,
                        new[] { WGL_COLOR_BITS_ARB }, outValues))
                    throw new Win32Exception();

                return outValues[0];
            }
            case GLAttribute.DepthSize:
            {
                var outValues = new int[1];
                if (!WGL.wglGetPixelFormatAttribivARB(_hDC, pixFmt, PFD_MAIN_PLANE,
                        new[] { WGL_DEPTH_BITS_ARB }, outValues))
                    throw new Win32Exception();

                return outValues[0];
            }
            case GLAttribute.RedSize:
            {
                var outValues = new int[1];
                if (!WGL.wglGetPixelFormatAttribivARB(_hDC, pixFmt, PFD_MAIN_PLANE,
                        new[] { WGL_RED_BITS_ARB }, outValues))
                    throw new Win32Exception();

                return outValues[0];
            }
            case GLAttribute.GreenSize:
            {
                var outValues = new int[1];
                if (!WGL.wglGetPixelFormatAttribivARB(_hDC, pixFmt, PFD_MAIN_PLANE,
                        new[] { WGL_GREEN_BITS_ARB }, outValues))
                    throw new Win32Exception();

                return outValues[0];
            }
            case GLAttribute.BlueSize:
            {
                var outValues = new int[1];
                if (!WGL.wglGetPixelFormatAttribivARB(_hDC, pixFmt, PFD_MAIN_PLANE,
                        new[] { WGL_BLUE_BITS_ARB }, outValues))
                    throw new Win32Exception();

                return outValues[0];
            }
            case GLAttribute.AlphaSize:
            {
                var outValues = new int[1];
                if (!WGL.wglGetPixelFormatAttribivARB(_hDC, pixFmt, PFD_MAIN_PLANE,
                        new[] { WGL_ALPHA_BITS_ARB }, outValues))
                    throw new Win32Exception();

                return outValues[0];
            }
            case GLAttribute.SwapControl:
            {
                return WGL.wglGetSwapIntervalEXT();
            }
            case GLAttribute.MultisampleBuffers:
            {
                var outValues = new int[1];
                if (!WGL.wglGetPixelFormatAttribivARB(_hDC, pixFmt, PFD_MAIN_PLANE,
                        new[] { WGL_SAMPLE_BUFFERS_ARB }, outValues))
                    throw new Win32Exception();

                return outValues[0];
            }
            case GLAttribute.MultisampleSamples:
            {
                var outValues = new int[1];
                if (!WGL.wglGetPixelFormatAttribivARB(_hDC, pixFmt, PFD_MAIN_PLANE,
                        new[] { WGL_SAMPLES_ARB }, outValues))
                    throw new Win32Exception();

                return outValues[0];
            }
            case GLAttribute.ContextMajorVersion:
            {
                return GL.GetInteger(GetPName.MajorVersion);
            }
            case GLAttribute.ContextMinorVersion:
            {
                return GL.GetInteger(GetPName.MinorVersion);
            }
            case GLAttribute.ContextProfileMask:
            {
                int extCount = GL.GetInteger(GetPName.NumExtensions);
                for (int i = 0; i < extCount; i++)
                {
                    if (GL.GetString(StringNameIndexed.Extensions, i) == "ARB_compatibility")
                    {
                        return (int) GLContextType.Compatibilty;
                    }
                }

                return (int) GLContextType.Core;
            }
        }

        throw new ArgumentException("Invalid GLAttribute");
    }

    public void ResizeWindow(Size size)
    {
        if (!GetWindowRect(_hWnd, out var winRect))
            throw new Win32Exception();

        MoveWindow(_hWnd, winRect.X, winRect.Y, size.Width, size.Height, false);
    }

    public void SetVisible(bool visible)
    {
        ShowWindow(_hWnd, visible ? SW_SHOWNA : SW_HIDE);
    }

    public IntPtr GetProcAddress(string symbol)
    {
        return wglGetProcAddress(symbol);
    }
}