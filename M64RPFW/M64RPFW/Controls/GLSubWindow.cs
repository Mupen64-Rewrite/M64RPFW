using System;
using System.Drawing;
using Eto;
using Eto.Forms;
using Eto.Threading;
using M64RPFW.Models.Emulation.Core;
using OpenTK;

namespace M64RPFW.Controls
{
    /// <summary>
    /// Represents a child window (or equivalent) that can be drawn on and
    /// managed separately from its parent window.
    /// </summary>
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
        
        /// <summary>
        /// Handler interface for the GLSubWindow widget.
        /// </summary>
        public interface IGLSubWindow : IHandler
        {
            /// <summary>
            /// (Re)create the OpenGL subwindow.
            /// </summary>
            /// <param name="size">size of the window</param>
            /// <param name="bitsPerPixel">Number of bits per pixel. Not exactly useful.</param>
            /// <param name="videoMode">Exists for consistency with Mupen64Plus API.</param>
            /// <param name="flags">Exists for consistency with Mupen64Plus API.</param>
            /// <returns></returns>
            Mupen64Plus.Error SetVideoMode(Size size, int bitsPerPixel, Mupen64Plus.VideoMode videoMode, Mupen64Plus.VideoFlags flags);
            /// <summary>
            /// Make the OpenGL subwindow's context current.
            /// </summary>
            void MakeCurrent();
            /// <summary>
            /// Set an OpenGL attribute if the window has not been created.
            /// </summary>
            /// <param name="attr">The OpenGL attribute to set</param>
            /// <param name="value">the value to set it to</param>
            /// <exception cref="InvalidOperationException">if the window has been created</exception>
            /// <returns>an error code compatible with Mupen64Plus</returns>
            Mupen64Plus.Error SetAttribute(Mupen64Plus.GLAttribute attr, int value);
            Mupen64Plus.Error SwapBuffers();
            /// <summary>
            /// Resize the subwindow.
            /// </summary>
            /// <param name="size">the size to resize to</param>
            /// <returns>an error code compatible with Mupen64Plus</returns>
            Mupen64Plus.Error ResizeWindow(Size size);
            /// <summary>
            /// Get the address of a symbol in this subwindow's OpenGL implementation.
            /// </summary>
            /// <param name="symbol">the symbol to lookup</param>
            /// <returns>a pointer to the symbol</returns>
            IntPtr GetProcAddress(string symbol);
            /// <summary>
            /// Remove the OpenGL subwindow.
            /// </summary>
            void CloseVideo();
        }

        
    }
}