using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.Models.Emulation.API.Plugins
{
    internal class RspPlugin : Plugin
    {
        public RspPlugin(Mupen64PlusTypes.EmulatorPluginType type, IntPtr handle) : base(type, handle)
        {
        }
    }
}
