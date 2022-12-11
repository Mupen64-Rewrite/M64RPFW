using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;
using static M64RPFW.Wpf.Interfaces.Win32PInvoke;

namespace M64RPFW.Wpf.Helpers;

public partial class Win32SubWindow
{
    private const string WINDOW_CLASS = "RPFW-Main";
    private static HBRUSH orange;
    
    private static LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        switch (uMsg)
        {
            case WM_NCHITTEST:
            {
                // equivalent to: return HTNOWHERE;
                return (LRESULT) 0;
            }
            case WM_SETFOCUS:
            {
                var parent = GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_HWNDPARENT);
                if (parent == IntPtr.Zero)
                    throw new Win32Exception();

                if (SetFocus((HWND) parent) == HWND.Null)
                    throw new Win32Exception();
                return (LRESULT) 0;
            }
            default:
                return DefWindowProc(hWnd, uMsg, wParam, lParam);
        }
    }
    
    static unsafe Win32SubWindow()
    {
        _refWndProc = WindowProc;

        fixed (char* pWindowClass = WINDOW_CLASS)
        {
            WNDCLASSEXW wndClass = new()
            {
                cbSize = (uint) Marshal.SizeOf<WNDCLASSEXW>(),
                style = CS_OWNDC | CS_HREDRAW | CS_VREDRAW,
                lpfnWndProc = _refWndProc,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = CurrentHInstanceRaw,
                hIcon = HICON.Null,
                hCursor = LoadCursor(HINSTANCE.Null, IDC_ARROW),
                hbrBackground = HBRUSH.Null,
                lpszMenuName = null,
                lpszClassName = pWindowClass,
                hIconSm = HICON.Null
            };

            RegisterClassEx(in wndClass);
        }
    }

    internal static readonly WNDPROC _refWndProc;
}