using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace M64RPFW.Views.Avalonia.Helpers.Platform;

internal static class WinAPIExt
{
    #region Get/SetWindowLongPtr

    [DllImport("USER32.dll", EntryPoint = "GetWindowLongPtrW")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    private static extern IntPtr _GetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex);

    [SupportedOSPlatform("windows5.0")]
    public static IntPtr GetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
    {
        return IntPtr.Size switch
        {
            4 => WinAPI.GetWindowLong(hWnd, nIndex),
            8 => _GetWindowLongPtr(hWnd, nIndex),
            _ => throw new NotSupportedException("Platform is not 32-bit or 64-bit")
        };
    }


    [DllImport("USER32.dll", EntryPoint = "SetWindowLongPtrW")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    private static extern IntPtr _SetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, IntPtr dwNewLong);

    [SupportedOSPlatform("windows5.0")]
    public static IntPtr SetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size switch
        {
            4 => WinAPI.SetWindowLong(hWnd, nIndex, (int) dwNewLong),
            8 => _SetWindowLongPtr(hWnd, nIndex, dwNewLong),
            _ => throw new NotSupportedException("Platform is not 32-bit or 64-bit")
        };
    }

    #endregion
}