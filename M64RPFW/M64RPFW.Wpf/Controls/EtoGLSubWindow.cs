using Eto.Wpf.Forms;
using M64RPFW.Models.Emulation.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.Wpf.Controls
{
    /// <summary>
    /// Interface binding <see cref="WpfGLSubWindow"/> to Eto.
    /// </summary>
    public class GLSubWindow : WpfControl<
        WpfGLSubWindow, M64RPFW.Controls.GLSubWindow, Eto.Forms.Control.ICallback>, M64RPFW.Controls.GLSubWindow.IGLSubWindow
    {
        public GLSubWindow()
        {
            Control = new WpfGLSubWindow();
        }

        public void CloseVideo()
        {
            Control.CloseVideo();
        }

        public Mupen64Plus.Error GetAttribute(Mupen64Plus.GLAttribute attr, ref int value)
        {
            return Control.GetAttribute(attr, ref value);
        }

        public IntPtr GetProcAddress(string symbol)
        {
            return Control.GetProcAddress(symbol);
        }

        public void MakeCurrent()
        {
            Control.MakeCurrent();
        }

        public Mupen64Plus.Error ResizeWindow(Size size)
        {
            Control.ResizeWindow(size);
            return Mupen64Plus.Error.Success;
        }

        public Mupen64Plus.Error SetAttribute(Mupen64Plus.GLAttribute attr, int value)
        {
            return Control.SetAttribute(attr, value);
        }

        public Mupen64Plus.Error SetVideoMode(Size size, int bitsPerPixel, Mupen64Plus.VideoMode videoMode, Mupen64Plus.VideoFlags flags)
        {
            Console.WriteLine($"VIDEXT: setting video mode with size {size.Width}x{size.Height}");
            return Control.SetVideoMode(size, bitsPerPixel, videoMode);
        }

        public Mupen64Plus.Error SwapBuffers()
        {

            Control.SwapBuffers();
            return Mupen64Plus.Error.Success;
        }
    }
}
