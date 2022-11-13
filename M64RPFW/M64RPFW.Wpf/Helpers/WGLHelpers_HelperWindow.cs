using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_EX_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;

namespace M64RPFW.Wpf.Helpers;

// This file contains the implementation of a helper window.
// This is needed to do such things as initializing WGL properly.

public partial class WGLHelpers
{
    private static readonly HINSTANCE _hInstance = (HINSTANCE) Marshal.GetHINSTANCE(typeof(WGLHelpers).Module);

    private static HWND _helperWindow;
    // Window class names should be short. This one stands for
    // RPFW.(HeLPer)
    private static Utf16CString _helperClassName = new("RPFW.HLP");
    private static Utf16CString _helperWindowTitle = new("RPFW WGL Helper");
    private static WNDCLASSEXW _helperWndclass = new()
    {
        cbSize = (uint) Marshal.SizeOf<WNDCLASSEXW>(),
        style = WNDCLASS_STYLES.CS_OWNDC,
        lpfnWndProc = (hWnd, uMsg, wParam, lParam) =>
        {
            // This lambda is left here for extensibility.
            // ReSharper disable once ConvertClosureToMethodGroup
            return DefWindowProc(hWnd, uMsg, wParam, lParam);
        },
        cbClsExtra = 0,
        cbWndExtra = 0,
        hInstance = _hInstance,
        hIcon = HICON.Null,
        hCursor = HCURSOR.Null,
        hbrBackground = (HBRUSH) (IntPtr) 1,
        lpszMenuName = null,
        lpszClassName = _helperClassName,
        hIconSm = HICON.Null
    };

    static unsafe WGLHelpers()
    {
        // Code adapted from GLFW:
        // https://github.com/glfw/glfw/blob/d299d9f78857e921b66bdab42c7ea27fe2e31810/src/win32_init.c#L375-L410
        
        if (RegisterClassEx(in _helperWndclass) == 0)
            throw new Win32Exception();

        _helperWindow = CreateWindowEx(WS_EX_OVERLAPPEDWINDOW, _helperClassName,
            _helperWindowTitle, 
            WS_CLIPSIBLINGS | WS_CLIPCHILDREN, 
            0, 0, 1, 1, HWND.Null, HMENU.Null, _hInstance);
        if (_helperWindow == HWND.Null)
            throw new Win32Exception();

        ShowWindow(_helperWindow, SW_HIDE);
        
        // Add cleanup code
        AppDomain.CurrentDomain.ProcessExit += delegate
        {
            DestroyWindow(_helperWindow);
            UnregisterClass(_helperClassName, _hInstance);
        };
    }
}