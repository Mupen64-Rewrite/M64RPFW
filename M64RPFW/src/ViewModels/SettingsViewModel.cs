using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.src.Containers;
using M64RPFW.src.Helpers;
using M64RPFW.UI.ViewModels.Extensions.Localization;
using System;

namespace M64RPFW.UI.ViewModels
{
    internal partial class SettingsViewModel : ObservableObject
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;

        internal SettingsViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;
        }

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
            string[] vParams = SettingsVirtualArgumentHelper.ParseVirtualArgument(virtualArguments);
            System.Reflection.PropertyInfo? prop = generalDependencyContainer.SettingsManager.GetSettings().GetType().GetProperty(vParams[0]);

            object? tValue = null;

            if (targetType == typeof(string))
                tValue = vParams[1];
            else if (targetType == typeof(int))
                tValue = int.Parse(vParams[1]);
            else if (targetType == typeof(uint))
                tValue = uint.Parse(vParams[1]);
            else if (targetType == typeof(bool))
                tValue = bool.Parse(vParams[1]);
            else
                throw new ArgumentException($"Invalid value type: {vParams[1]}");

            prop.SetValue(generalDependencyContainer.SettingsManager.GetSettings(), tValue, null);
            generalDependencyContainer.SettingsManager.GetSettings().Save();
        }

        [RelayCommand]
        private void SetCulture(string cultureString) => generalDependencyContainer.LocalizationProvider.SetLocale(cultureString);
        [RelayCommand]
        private void SetTheme(string themeString) => generalDependencyContainer.ThemeManager.SetTheme(themeString);


        private bool ShowFileDialogAndPickLibraryPath(out string path)
        {
            (string ReturnedPath, bool Cancelled) ret = generalDependencyContainer.FileDialogProvider.OpenFileDialogPrompt(new[] { "dll" });
            path = ret.ReturnedPath;
            return !ret.Cancelled;
        }

        [RelayCommand]
        private void BrowseCoreLibraryPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
            {
                generalDependencyContainer.SettingsManager.GetSettings().CoreLibraryPath = path;
            }
        }


        [RelayCommand]
        private void BrowseVideoPluginPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
                generalDependencyContainer.SettingsManager.GetSettings().VideoPluginPath = path;
        }
        [RelayCommand]
        private void BrowseAudioPluginPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
                generalDependencyContainer.SettingsManager.GetSettings().AudioPluginPath = path;
        }
        [RelayCommand]
        private void BrowseInputPluginPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
                generalDependencyContainer.SettingsManager.GetSettings().InputPluginPath = path;
        }
        [RelayCommand]
        private void BrowseRSPPluginPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
                generalDependencyContainer.SettingsManager.GetSettings().RSPPluginPath = path;
        }
    }
}
