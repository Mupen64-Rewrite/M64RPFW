using M64RPFWAvalonia.Properties;
using System.Globalization;

namespace M64RPFWAvalonia.UI.ViewModels.Extensions.Localization
{
    public static class LocalizationManager
    {
        public static void SetCulture(string cultureString)
        {
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(cultureString);
            }
            catch
            {
                culture = CultureInfo.CurrentCulture;
            }
            System.Threading.Thread.CurrentThread.CurrentCulture =
            System.Threading.Thread.CurrentThread.CurrentUICulture =
            LocalizationSource.Instance.CurrentCulture = culture;
            Settings.Default.Culture = cultureString;
            Settings.Default.Save();
        }
    }
}
