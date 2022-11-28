using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using static Windows.Win32.PInvoke;

namespace M64PRR.Wpf.Interfaces;

/// <summary>
/// A SafeHandle that does absolutely nothing to clean up its contents.
/// </summary>
internal sealed class BorrowedHandle : SafeHandle
{
    public BorrowedHandle(IntPtr val) : base(IntPtr.Zero, true)
    {
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        return true;
    }
}

/// <summary>
/// A SafeHandle that uses ReleaseDC to release a DC associated with a window.
/// </summary>
internal sealed class ReleaseDCSafeHandle : DeleteDCSafeHandle
{
    public ReleaseDCSafeHandle(HWND hWnd, IntPtr handle)
        : base(handle)
    {
        HWnd = hWnd;
    }

    public HWND HWnd { get; }

    protected override bool ReleaseHandle() => ReleaseDC(HWnd, (HDC) handle) != 0;
}

internal sealed class DestroyWindowSafeHandle : SafeHandle
{
    public DestroyWindowSafeHandle(HWND hWnd) : base(IntPtr.Zero, true)
    {
        handle = hWnd;
    }

    public static implicit operator HWND(DestroyWindowSafeHandle safeHandle)
    {
        return (HWND) safeHandle.handle;
    }

    protected override bool ReleaseHandle()
    {
        return DestroyWindow((HWND) handle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}