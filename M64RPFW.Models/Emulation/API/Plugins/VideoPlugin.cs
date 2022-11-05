using M64RPFW.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static M64RPFW.Models.Emulation.API.Mupen64PlusDelegates;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.API.Plugins
{
    internal class VideoPlugin : Plugin
    {
        public VideoPlugin(EmulatorPluginType type, IntPtr handle) : base(type, handle)
        {
            ReadScreen2 = GetDelegateFromLibrary<ReadScreen2Delegate>(handle);
            ReadScreen2Res = GetDelegateFromLibrary<ReadScreen2ResDelegate>(handle);

            IntPtr funcPtr = GetProcAddress(handle, "GetScreenTextureID");
            if (funcPtr != IntPtr.Zero)
            {
                GetScreenTextureID = (GetScreenTextureIDDelegate)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(GetScreenTextureIDDelegate));
            }
        }

        internal ReadScreen2Delegate ReadScreen2 { get; private set; }
        internal ReadScreen2ResDelegate ReadScreen2Res { get; private set; }
        internal GetScreenTextureIDDelegate GetScreenTextureID { get; private set; }
    }
}
