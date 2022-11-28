using System;
using Windows.Win32;
using OpenTK;

namespace M64PRR.Wpf.Interfaces;

public class WGLBindingsContext : IBindingsContext
{
    public IntPtr GetProcAddress(string procName)
    {
        return PInvoke.wglGetProcAddress(procName);
    }
}