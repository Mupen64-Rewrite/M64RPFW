using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace M64RPFW.Views.Avalonia.Controls.Helpers.Platform;

internal static class Win32Helpers
{
    private static readonly Dictionary<HWND, (HWND ParentHwnd, IntPtr OriginalWndProc)> _windowDictionary = new();

    private const int WM_MOUSEFIRST = 0x0200;
    private const int WM_MOUSELAST = 0x020D;
    private const int GWLP_WNDPROC = -4;


    // NOTE: this will crash on 32 bit
    // get/setwindowlongptr dont exist there

    private delegate IntPtr WindowProcDelegate(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newWndProc);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    public static void EnableMousePassthrough(HWND childHwnd, HWND parentHwnd)
    {
        // get current wndproc and store it
        IntPtr currentWndProc = GetWindowLongPtr(childHwnd, GWLP_WNDPROC);
        _windowDictionary[childHwnd] = (parentHwnd, currentWndProc);

        // swap out original wndproc for ours
        WindowProcDelegate windowProcDelegate = ForwardWindowProc;
        SetWindowLongPtr(childHwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(windowProcDelegate));
    }

    private static IntPtr ForwardWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
    {
        var entry = _windowDictionary[new HWND(hwnd)];

        if (msg is >= WM_MOUSEFIRST and <= WM_MOUSELAST)
        {
            return CallWindowProc(entry.OriginalWndProc, entry.ParentHwnd, msg, wParam, lParam);
        }

        return CallWindowProc(entry.OriginalWndProc, hwnd, msg, wParam, lParam);
    }

}