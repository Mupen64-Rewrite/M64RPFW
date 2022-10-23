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
using Xceed.Wpf.AvalonDock.Controls;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_STYLE;
using static Windows.Win32.UI.WindowsAndMessaging.WNDCLASS_STYLES;

namespace M64RPFW.Wpf.Controls
{
    public unsafe class WpfGLSubWindow : UserControl
    {
        public WpfGLSubWindow()
        {
            
        }

        private void OnLoaded(LoadedEventArgs evt)
        {

        }

        private void OnUnloaded(RoutedEventArgs evt)
        {

        }
        private WGLSubwindow? _glWindow;
    }
}
