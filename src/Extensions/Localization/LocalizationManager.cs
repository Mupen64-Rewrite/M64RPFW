using System.Globalization;

namespace M64RPFW.UI.ViewModels.Extensions.Localization
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
            Properties.Settings.Default.Culture = cultureString;
            Properties.Settings.Default.Save();
        }
    }
}
