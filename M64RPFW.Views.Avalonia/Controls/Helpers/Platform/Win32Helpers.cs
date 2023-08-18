using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using M64RPFW.Views.Avalonia.Helpers.Platform;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_EX_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX;

namespace M64RPFW.Views.Avalonia.Controls.Helpers.Platform;

internal static class Win32Helpers
{
    [SupportedOSPlatform("windows5.0")]
    private static LRESULT CustomWndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
            case WinAPI.WM_NCHITTEST:
                return (LRESULT) WinAPI.HTTRANSPARENT;
            case WinAPI.WM_LBUTTONDOWN:
                Console.WriteLine("wndproc go clicc");
                return WinAPI.DefWindowProc(hWnd, msg, wParam, lParam);
            default:
                return WinAPI.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    [SupportedOSPlatform("windows5.0")]
    private static WNDPROC? _customWndProcInstance;
    private static IntPtr _customWndProcPointer;

    [SupportedOSPlatform("windows5.0")]
    public static void PlatformWindowSetup(HWND hWnd)
    {
        if (_customWndProcInstance == null)
        {
            _customWndProcInstance = CustomWndProc;
            _customWndProcPointer = Marshal.GetFunctionPointerForDelegate(_customWndProcInstance);
        }

        WinAPIExt.SetWindowLongPtr(hWnd, GWLP_WNDPROC, _customWndProcPointer);
        // WinAPI.SetWindowLong(hWnd, GWL_EXSTYLE, (int) WS_EX_NOACTIVATE);
        // WinAPI.EnableWindow(hWnd, false);

    }
}