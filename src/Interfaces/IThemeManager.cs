using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.src.Interfaces
{
    internal interface IThemeManager
    {
        internal Themes.Themes GetTheme();
        internal void SetTheme(string themeName);
    }
}
