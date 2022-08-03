using System.Drawing;

namespace M64PRR.Gtk.Helpers;

public interface IOpenGLWindow
{
    void MakeCurrent();
    void SwapBuffers();
    void ResizeWindow(Size size);
}