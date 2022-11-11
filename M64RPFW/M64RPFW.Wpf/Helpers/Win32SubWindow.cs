using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD;

namespace M64RPFW.Wpf.Helpers;

public partial class Win32SubWindow : IDisposable
{
    
    public Win32SubWindow(System.Windows.Window parent, Size size, IDictionary<GLAttribute, int> attrs)
    {
        GCHandle attrsHandle = GCHandle.Alloc(attrs);
        try
        {
        }
        finally
        {
            attrsHandle.Free();
        }

    }
    
    void MakeCurrent()
    {
        wglMakeCurrent(_hDC, _hGLRC);
    }

    void SwapBuffers()
    {
    }
    
    public void Dispose()
    {
    }

    void SetPosition(Point pos)
    {
    }

    int GetAttribute(GLAttribute attr)
    {
        
    }

    void ResizeWindow(Size size)
    {
    }

    void SetVisible(bool visible)
    {
        ShowWindow(_hWnd, visible ? SW_SHOWNA : SW_HIDE);
    }

    IntPtr GetProcAddress(string symbol)
    {
        return wglGetProcAddress(symbol);
    }
}