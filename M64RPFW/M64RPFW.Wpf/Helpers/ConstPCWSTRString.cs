using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;

namespace M64RPFW.Wpf.Helpers
{
    /// <summary>
    /// Wrapper class that exposes a <c>const wchar_t*</c> from a <see cref="string"/>
    /// </summary>
    internal unsafe class ConstPCWSTRString
    {
        public ConstPCWSTRString(string s)
        {
            data = Marshal.StringToHGlobalUni(s);
        }

        ~ConstPCWSTRString()
        {
            Marshal.FreeHGlobal(data);
        }

        public static implicit operator ConstPCWSTRString(string s) => new ConstPCWSTRString(s);

        public static implicit operator PCWSTR(ConstPCWSTRString s)
        {
            return new PCWSTR((char*) s.data.ToPointer());
        }

        IntPtr data;
    }
}
