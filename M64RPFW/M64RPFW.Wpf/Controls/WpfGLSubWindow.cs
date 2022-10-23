using M64RPFW.Wpf.Helpers;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;

namespace M64RPFW.Wpf.Controls
{
    public unsafe class WpfGLSubWindow : UserControl
    {
        private static readonly ConstPCWSTRString Win32ClassName = "m64rpfw-opengl-window";
        private static readonly ConstPCWSTRString BasicTitle = "M64RPFW OpenGL output";
        public WpfGLSubWindow()
        {
            
        }

        private void OnLoaded(LoadedEventArgs evt)
        {
            _childHwnd = InitChildHWND();
        }

        private void OnUnloaded(RoutedEventArgs evt)
        {
            DestroyWindow(_childHwnd);
        }

        private HWND InitChildHWND()
        {
            WNDCLASSW wc = new WNDCLASSW();
            wc.lpfnWndProc = WindowProcedure;
            wc.hInstance = (HINSTANCE)Marshal.GetHINSTANCE(typeof(WpfGLSubWindow).Module);
            wc.lpszClassName = Win32ClassName;
            wc.style = CS_OWNDC;

            RegisterClass(in wc);

            return CreateWindowEx(0, Win32ClassName, BasicTitle, WS_CHILD, 0, 0, 100, 100, _parentHwnd, (HMENU)IntPtr.Zero, wc.hInstance);
        }

        private LRESULT WindowProcedure(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
        {
            return (LRESULT)0;
        }

        private HWND _parentHwnd => (HWND) new WindowInteropHelper(Window.GetWindow(this)).Owner;
        private HWND _childHwnd { get; set; } = (HWND)IntPtr.Zero;
    }
}
