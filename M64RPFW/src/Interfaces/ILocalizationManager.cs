using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.src.Interfaces
{
    public interface ILocalizationManager
    {
        public string GetString(string key);
        public void SetLocale(string localeKey);
    }
}
