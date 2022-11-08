using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

using static Windows.Win32.PInvoke;

namespace M64RPFW.Wpf.Helpers
{
    public static class Win32Extras
    {
        [DllImport("USER32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern long SetWindowLongPtr_Internal(HWND window, WINDOW_LONG_PTR_INDEX setting, long value);

        internal static nint SetWindowLongPtr(HWND window, WINDOW_LONG_PTR_INDEX setting, nint value)
        {
            if (IntPtr.Size == 4)
            {
                return (nint) SetWindowLong(window, setting, (int) value);
            }
            else if (IntPtr.Size == 8)
            {
                return (nint) SetWindowLongPtr_Internal(window, setting, (long) value);
            }
            throw new PlatformNotSupportedException("WhAAaaAT?");
        }
    }
}