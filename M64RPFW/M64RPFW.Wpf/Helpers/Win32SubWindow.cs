using System;
using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using M64RPFW.Misc;
using M64RPFW.Models.Emulation.Core;

namespace M64PRR.Wpf.Helpers;

public class Win32SubWindow : IOpenGLWindow
{
    public Win32SubWindow()
    {
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void MakeCurrent()
    {
        throw new NotImplementedException();
    }

    public void SwapBuffers()
    {
        throw new NotImplementedException();
    }

    public void SetPosition(Point pos)
    {
        throw new NotImplementedException();
    }

    public int GetAttribute(Mupen64Plus.GLAttribute attr)
    {
        throw new NotImplementedException();
    }

    public void ResizeWindow(Size size)
    {
        throw new NotImplementedException();
    }

    public void SetVisible(bool visible)
    {
        throw new NotImplementedException();
    }

    public IntPtr GetProcAddress(string symbol)
    {
        throw new NotImplementedException();
    }
    
    
}