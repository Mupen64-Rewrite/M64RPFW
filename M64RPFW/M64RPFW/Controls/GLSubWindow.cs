using System;
using System.Drawing;
using Eto;
using Eto.Forms;
using Eto.Threading;
using M64RPFW.Models.Emulation.Core;
using OpenTK;

namespace M64RPFW.Controls
{
    [Handler(typeof(IGLSubWindow))]
    public class GLSubWindow : Control, IBindingsContext
    {
        private new IGLSubWindow Handler => (IGLSubWindow) base.Handler;

        public Mupen64Plus.Error SetVideoMode(Size size, int bitsPerPixel, Mupen64Plus.VideoMode videoMode, Mupen64Plus.VideoFlags flags)
        {
            Mupen64Plus.Error err = Application.Instance.Invoke(() => Handler.SetVideoMode(size, bitsPerPixel, videoMode, flags));
            if (err != Mupen64Plus.Error.Success)
                return err;
            Handler.MakeCurrent();
            return Mupen64Plus.Error.Success;
        }

        public Mupen64Plus.Error SetAttribute(Mupen64Plus.GLAttribute attr, int value)
        {
            return Application.Instance.Invoke(() => Handler.SetAttribute(attr, value));
        }

        public Mupen64Plus.Error SwapBuffers()
        {
            return Handler.SwapBuffers();
        }

        public Mupen64Plus.Error ResizeWindow(Size size)
        {
            return Application.Instance.Invoke(() => Handler.ResizeWindow(size));
        }
        
        public IntPtr GetProcAddress(string procName)
        {
            return Handler.GetProcAddress(procName);
        }

        public void CloseVideo()
        {
            Handler.CloseVideo();
        }
        
        public interface IGLSubWindow : IHandler
        {
            Mupen64Plus.Error SetVideoMode(Size size, int bitsPerPixel, Mupen64Plus.VideoMode videoMode, Mupen64Plus.VideoFlags flags);
            void MakeCurrent();
            Mupen64Plus.Error SetAttribute(Mupen64Plus.GLAttribute attr, int value);
            Mupen64Plus.Error SwapBuffers();
            Mupen64Plus.Error ResizeWindow(Size size);
            IntPtr GetProcAddress(string symbol);
            void CloseVideo();
        }

        
    }
}