using System;
using System.Drawing;
using static M64RPFW.Models.Emulation.Core.Mupen64Plus;

namespace M64RPFW.Misc;

public interface IOpenGLWindow : IDisposable
{
    void MakeCurrent();
    void SwapBuffers();
    void SetPosition(Point pos);
    int GetAttribute(GLAttribute attr);
    void ResizeWindow(Size size);
    void SetVisible(bool visible);
    IntPtr GetProcAddress(string symbol);
}