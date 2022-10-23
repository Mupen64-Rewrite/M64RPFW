using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Windows.Win32.PInvoke;

namespace M64RPFW.Wpf.Helpers
{
    internal class WGLBindingsContext : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return (IntPtr) wglGetProcAddress(procName);
        }
    }
}
