using MaterialDesignThemes.Wpf;

namespace M64RPFW.UI.ViewModels.Helpers
{
    public static class ThemeHelper
    {
        public static void SetTheme(string themeString)
        {
            PaletteHelper paletteHelper = new();
            ITheme theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(themeString == "Dark" ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
            Properties.Settings.Default.Theme = themeString;
            Properties.Settings.Default.Save();
        }
    }
}
