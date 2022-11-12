﻿using System;
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
        
        
        [DllImport("USER32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern long GetWindowLongPtr_Internal(HWND window, WINDOW_LONG_PTR_INDEX setting);

        internal static long SetWindowLongPtr(HWND window, WINDOW_LONG_PTR_INDEX setting, long value)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLong(window, setting, (int) value);
            }

            if (IntPtr.Size == 8)
            {
                return SetWindowLongPtr_Internal(window, setting, value);
            }
            
            throw new PlatformNotSupportedException("This version of Windows is not supported.");
        }

        internal static long GetWindowLongPtr(HWND window, WINDOW_LONG_PTR_INDEX setting)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong(window, setting);
            }

            if (IntPtr.Size == 8)
            {
                return GetWindowLongPtr_Internal(window, setting);
            }
            
            throw new PlatformNotSupportedException("This version of Windows is not supported.");
        }
    }
}