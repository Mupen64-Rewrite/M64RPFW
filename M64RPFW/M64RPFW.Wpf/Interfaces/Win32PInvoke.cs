using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace M64RPFW.Wpf.Interfaces;

internal static class Win32PInvoke
{
    #region Get/SetWindowLongPtr

    [DllImport("USER32.dll", EntryPoint = "GetWindowLongPtrW")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    private static extern IntPtr GetWindowLongPtr_Internal(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex);

    public static IntPtr GetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex)
    {
        return IntPtr.Size switch
        {
            4 => (IntPtr) GetWindowLong(hWnd, nIndex),
            8 => GetWindowLongPtr_Internal(hWnd, nIndex),
            _ => throw new NotSupportedException("Platform is not 32-bit or 64-bit")
        };
    }


    [DllImport("USER32.dll", EntryPoint = "GetWindowLongPtrW")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    private static extern IntPtr SetWindowLongPtr_Internal(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, IntPtr dwNewLong);

    public static IntPtr SetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size switch
        {
            4 => (IntPtr) SetWindowLong(hWnd, nIndex, (int) dwNewLong),
            8 => SetWindowLongPtr_Internal(hWnd, nIndex, dwNewLong),
            _ => throw new NotSupportedException("Platform is not 32-bit or 64-bit")
        };
    }

    #endregion

    #region Get the current HINSTANCE

    public static readonly BorrowedHandle CurrentHInstance = new(Marshal.GetHINSTANCE(typeof(Win32PInvoke).Module));
    public static HINSTANCE CurrentHInstanceRaw => (HINSTANCE) CurrentHInstance.DangerousGetHandle();

    #endregion

    #region GetDC with SafeHandle

    public static ReleaseDCSafeHandle GetDC_SafeHandle2(HWND hWnd)
    {
        HDC dc = GetDC(hWnd);
        return new ReleaseDCSafeHandle(hWnd, dc);
    }

    #endregion

    #region CreateWindowEx with SafeHandle

    public static unsafe DestroyWindowSafeHandle CreateWindowEx_SafeHandle2(WINDOW_EX_STYLE dwExStyle,
        string lpClassName, string lpWindowName, WINDOW_STYLE dwStyle, int X, int Y, int nWidth, int nHeight,
        HWND hWndParent, SafeHandle? hMenu, SafeHandle? hInstance, void* lpParam = default)
    {
        HWND hWnd = CreateWindowEx(dwExStyle, lpClassName, lpWindowName, dwStyle, X, Y, nWidth, nHeight, hWndParent, hMenu,
            hInstance, lpParam);
        return new DestroyWindowSafeHandle(hWnd);
    }

    #endregion
}