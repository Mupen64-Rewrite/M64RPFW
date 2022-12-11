using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.OpenGL;
using Windows.Win32.UI.WindowsAndMessaging;
using M64RPFW.Misc;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Wpf.Interfaces;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_EX_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static M64RPFW.Wpf.Interfaces.Win32PInvoke;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace M64RPFW.Wpf.Helpers;

public partial class Win32SubWindow : IOpenGLWindow
{
    public unsafe Win32SubWindow(Window parent, Size size, Dictionary<Mupen64Plus.GLAttribute, int> attrs)
    {
        HWND parentHWnd = (HWND) new WindowInteropHelper(parent).Handle;
        
        _window = CreateWindowEx(0, WINDOW_CLASS, "M64RPFW Output",
            WS_CHILD, 0, 0, size.Width, size.Height,
            parentHWnd, null, CurrentHInstance, null);
        if (_window == HWND.Null)
            throw new Win32Exception();

        _dc = GetDC_SafeHandle2(_window);

        int pixFmt = WGLHelpers.ChoosePixelFormatM64P(attrs);
        PIXELFORMATDESCRIPTOR pfd;
        if (DescribePixelFormat(_dc, (PFD_PIXEL_TYPE) pixFmt, (uint) Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(), &pfd) == 0)
            throw new Win32Exception();

        if (!SetPixelFormat(_dc, pixFmt, in pfd))
            throw new Win32Exception();

        _glRC = WGLHelpers.CreateContextM64P(_dc, attrs);
        {
            if (attrs.TryGetValue(Mupen64Plus.GLAttribute.SwapControl, out int value))
            {
                if (!WGLHelpers.IsWGLExtensionSupported("WGL_EXT_swap_control"))
                    throw new InvalidOperationException("Swap intervals are not supported");
                WGL.SwapIntervalEXT(value);
            }
        }
        ShowWindow(_window, SW_SHOWNA);
    }

    ~Win32SubWindow()
    {
        Dispose(false);
    }
    
    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool fromDispose)
    {
        if (fromDispose)
        {
            _glRC.Dispose();
            _dc.Dispose();
        }

        Eto.Forms.Application.Instance.InvokeAsync(() =>
        {
            if (!DestroyWindow(_window))
            {
                var exc = new Win32Exception();
                Console.WriteLine($"DestroyWindow() failed: {exc}");
            }
        });
    }

    public void MakeCurrent()
    {
        if (!wglMakeCurrent(_dc, _glRC))
        {
            var err = new Win32Exception();
            Console.Error.WriteLine($"wglMakeCurrent failed ({err.NativeErrorCode}, {err.Message})");
            throw err;
        }
    }

    public void SwapBuffers()
    {
        if (!PInvoke.SwapBuffers(_dc))
        {
            throw new Win32Exception();
        }
    }

    public void SetPosition(Point pos)
    {
        GetWindowRect(_window, out var winRect);
        MoveWindow(_window, pos.X, pos.Y, winRect.Width, winRect.Height, false);
    }

    public int GetAttribute(Mupen64Plus.GLAttribute attr)
    {
        throw new NotImplementedException();
    }

    public void ResizeWindow(Size size)
    {
        GetWindowRect(_window, out var winRect);
        MoveWindow(_window, winRect.X, winRect.Y, size.Width, size.Height, false);
    }

    public void SetVisible(bool visible)
    {
        SHOW_WINDOW_CMD cmd = visible ? SW_SHOWNA : SW_HIDE;
        ShowWindow(_window, cmd);
    }

    public IntPtr GetProcAddress(string symbol)
    {
        if (wglGetCurrentContext() != _glRC.DangerousGetHandle())
        {
            wglMakeCurrent(_dc, _glRC);
        }
        return wglGetProcAddress(symbol);
    }

    private HWND _window;
    private SafeHandle _dc;
    private SafeHandle _glRC;
}