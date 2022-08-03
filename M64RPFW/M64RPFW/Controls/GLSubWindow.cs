using System.Drawing;
using Eto;
using Eto.Forms;

namespace M64PRR.Controls
{
    [Handler(typeof(IGLSubWindow))]
    public class GLSubWindow : Control
    {
        private new IGLSubWindow Handler => (IGLSubWindow) base.Handler;

        public void MakeCurrent()
        {
            
        }
        public interface IGLSubWindow : IHandler
        {
            void MakeCurrent();
            void SwapBuffers();
            void ResizeWindow(Size size);
        }
    }
}