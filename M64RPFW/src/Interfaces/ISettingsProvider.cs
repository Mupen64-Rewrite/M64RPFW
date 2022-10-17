using M64RPFW.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.src.Interfaces
{
    public interface ISettingsProvider
    {
        internal Settings GetSettings();
    }
}
