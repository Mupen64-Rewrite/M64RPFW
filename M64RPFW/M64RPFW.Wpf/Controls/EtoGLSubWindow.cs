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
    public class GLSubWindow : WpfControl<
        WpfGLSubWindow, M64RPFW.Controls.GLSubWindow, Eto.Forms.Control.ICallback>, M64RPFW.Controls.GLSubWindow.IGLSubWindow
    {
        public GLSubWindow()
        {
            Control = new WpfGLSubWindow();
        }

        public void CloseVideo()
        {
            throw new NotImplementedException();
        }

        public Mupen64Plus.Error GetAttribute(Mupen64Plus.GLAttribute attr, ref int value)
        {
            throw new NotImplementedException();
        }

        public IntPtr GetProcAddress(string symbol)
        {
            throw new NotImplementedException();
        }

        public void MakeCurrent()
        {
            throw new NotImplementedException();
        }

        public Mupen64Plus.Error ResizeWindow(Size size)
        {
            throw new NotImplementedException();
        }

        public Mupen64Plus.Error SetAttribute(Mupen64Plus.GLAttribute attr, int value)
        {
            throw new NotImplementedException();
        }

        public Mupen64Plus.Error SetVideoMode(Size size, int bitsPerPixel, Mupen64Plus.VideoMode videoMode, Mupen64Plus.VideoFlags flags)
        {
            throw new NotImplementedException();
        }

        public Mupen64Plus.Error SwapBuffers()
        {
            throw new NotImplementedException();
        }
    }
}
