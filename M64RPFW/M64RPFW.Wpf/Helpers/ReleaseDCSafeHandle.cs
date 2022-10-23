using System;
using System.Collections.Generic;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Gdi;
using static Windows.Win32.PInvoke;

namespace M64RPFW.Wpf.Helpers
{
    internal class ReleaseDCSafeHandle : DeleteDCSafeHandle
    {
        internal ReleaseDCSafeHandle(HWND hWnd, HDC handle)
            : base(handle)
        {
            HWnd = hWnd;
        }
        public HWND HWnd { get; }

        protected override bool ReleaseHandle() => ReleaseDC(HWnd, (HDC) handle) != 0;
    }
}
