using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Helpers;
using System;

namespace M64RPFW.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;

        public SettingsViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;
        }

        // TODO: rewrite
        // this is unmaintainable!!!

        [RelayCommand]
        private void SetSettingsPropertyInteger(string virtualArguments) => SetSettingsProperty<int>(virtualArguments);

        [RelayCommand]
        private void SetSettingsPropertyBool(string virtualArguments) => SetSettingsProperty<bool>(virtualArguments);

        private void SetSettingsProperty<T>(string virtualArguments)
        {
            string[] vParams = SettingsVirtualArgumentHelper.ParseVirtualArgument(virtualArguments);

            if (typeof(T) == typeof(bool))
            {
                generalDependencyContainer.SettingsManager.SetSetting(vParams[0], bool.Parse(vParams[1].ToString()));
            }
            else if (typeof(T) == typeof(int))
            {
                generalDependencyContainer.SettingsManager.SetSetting(vParams[0], int.Parse(vParams[1].ToString()));
            }
            else if (typeof(T) == typeof(string))
            {
                generalDependencyContainer.SettingsManager.SetSetting(vParams[0], vParams[1].ToString());
            }
            else
            {
                throw new Exception($"Could not resolve VirtualArgument");
            }

        }

        [RelayCommand]
        private void SetCulture(string cultureString)
        {
            generalDependencyContainer.LocalizationProvider.SetLocale(cultureString);
        }

        [RelayCommand]
        private void SetTheme(string themeString)
        {
            generalDependencyContainer.ThemeManager.SetTheme(themeString);
        }

        private bool ShowFileDialogAndPickLibraryPath(out string path)
        {
            (string ReturnedPath, bool Cancelled) = generalDependencyContainer.FileDialogProvider.OpenFileDialogPrompt(new[] { "dll" });
            path = ReturnedPath;
            return !Cancelled;
        }

        [RelayCommand]
        private void BrowseCoreLibraryPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
            {
                generalDependencyContainer.SettingsManager.SetSetting<string>("CoreLibraryPath", path, true);
            }
        }


        [RelayCommand]
        private void BrowseVideoPluginPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
            {
                generalDependencyContainer.SettingsManager.SetSetting<string>("VideoPluginPath", path, true);
            }
        }
        [RelayCommand]
        private void BrowseAudioPluginPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
            {
                generalDependencyContainer.SettingsManager.SetSetting<string>("AudioPluginPath", path, true);
            }
        }
        [RelayCommand]
        private void BrowseInputPluginPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
            {
                generalDependencyContainer.SettingsManager.SetSetting<string>("InputPluginPath", path, true);
            }
        }
        [RelayCommand]
        private void BrowseRSPPluginPath()
        {
            if (ShowFileDialogAndPickLibraryPath(out string? path))
            {
                generalDependencyContainer.SettingsManager.SetSetting<string>("RSPPluginPath", path, true);
            }
        }
    }
}
