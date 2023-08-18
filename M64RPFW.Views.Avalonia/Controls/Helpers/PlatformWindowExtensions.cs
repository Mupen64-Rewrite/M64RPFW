using System;
using System.Diagnostics;
using Windows.Win32.Foundation;
using Avalonia.Platform;
using M64RPFW.Views.Avalonia.Controls.Helpers.Platform;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public static class PlatformWindowExtensions
{
    public static void PlatformWindowSetup(this IPlatformHandle platformHandle)
    {
        switch (platformHandle.HandleDescriptor)
        {
            case "HWND":
                Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(5));
                Win32Helpers.PlatformWindowSetup((HWND) platformHandle.Handle);
                break;
            case "XID":
                X11Helpers.PlatformWindowSetup((uint) platformHandle.Handle);
                break;
        }
    }
}