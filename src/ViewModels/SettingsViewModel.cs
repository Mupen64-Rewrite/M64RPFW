using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models;
using M64RPFW.UI.ViewModels.Extensions.Localization;
using M64RPFW.UI.ViewModels.Helpers;
using System;

namespace M64RPFW.UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {

        // We cannot use generics :/
        // Is it time to write our own settings system and not use this built in archaic 2009 garbage
        [RelayCommand]
        private void SetSettingsPropertyInteger(string virtualArguments)
        {
            SetSettingsProperty(virtualArguments, typeof(int));
        }

        [RelayCommand]
        private void SetSettingsPropertyBool(string virtualArguments)
        {
            SetSettingsProperty(virtualArguments, typeof(bool));
        }

        // this sucks ass, but we have to deal with this until settings rewrite
        private void SetSettingsProperty(string virtualArguments, Type targetType)
        {
            var vParams = SettingsVirtualArgumentHelper.ParseVirtualArgument(virtualArguments);
            var prop = Properties.Settings.Default.GetType().GetProperty(vParams[0]);

            object? tValue = null;

            if (targetType == typeof(string))
            {
                tValue = vParams[1];
            }
            else if (targetType == typeof(int))
            {
                tValue = int.Parse(vParams[1]);
            }
            else if (targetType == typeof(uint))
            {
                tValue = uint.Parse(vParams[1]);
            }
            else if (targetType == typeof(bool))
            {
                tValue = bool.Parse(vParams[1]);
            }
            else
            {
                throw new ArgumentException($"Invalid value type: {vParams[1]}");
            }

            prop.SetValue(Properties.Settings.Default, tValue, null);
            Properties.Settings.Default.Save();
        }

        [RelayCommand]
        private void SetCulture(string cultureString) => LocalizationManager.SetCulture(cultureString);
        [RelayCommand]
        private void SetTheme(string themeString) => ThemeHelper.SetTheme(themeString);

    }
}
