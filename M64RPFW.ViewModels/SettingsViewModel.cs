using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Helpers;

namespace M64RPFW.ViewModels;

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
    private void SetSettingsPropertyInteger(string virtualArguments)
    {
        SetSettingsProperty<int>(virtualArguments);
    }

    [RelayCommand]
    private void SetSettingsPropertyBool(string virtualArguments)
    {
        SetSettingsProperty<bool>(virtualArguments);
    }

    private void SetSettingsProperty<T>(string virtualArguments)
    {
        var vParams = SettingsVirtualArgumentHelper.ParseVirtualArgument(virtualArguments);

        if (typeof(T) == typeof(bool))
            generalDependencyContainer.SettingsService.Set(vParams[0], bool.Parse(vParams[1]));
        else if (typeof(T) == typeof(int))
            generalDependencyContainer.SettingsService.Set(vParams[0], int.Parse(vParams[1]));
        else if (typeof(T) == typeof(string))
            generalDependencyContainer.SettingsService.Set(vParams[0], vParams[1]);
        else
            throw new Exception("Could not resolve VirtualArgument");
    }

    [RelayCommand]
    private void SetCulture(string cultureString)
    {
        generalDependencyContainer.LocalizationService.SetLocale(cultureString);
    }

    [RelayCommand]
    private void SetTheme(string themeString)
    {
        generalDependencyContainer.ThemeService.Set(themeString);
    }

    private async Task<(bool Succeeded, string Path)> ShowFileDialogAndPickPath()
    {
        var file = await generalDependencyContainer.FilesService.TryPickOpenFileAsync(new[] { "dll" });
        return file != null ? (true, file.Path) : ((bool Succeeded, string Path))(false, null);
    }

    [RelayCommand]
    private async void BrowseCoreLibraryPath()
    {
        var result = await ShowFileDialogAndPickPath();
        if (result.Succeeded) generalDependencyContainer.SettingsService.Set("CoreLibraryPath", result.Path, true);
    }


    [RelayCommand]
    private async void BrowseVideoPluginPath()
    {
        var result = await ShowFileDialogAndPickPath();
        if (result.Succeeded) generalDependencyContainer.SettingsService.Set("VideoPluginPath", result.Path, true);
    }

    [RelayCommand]
    private async void BrowseAudioPluginPath()
    {
        var result = await ShowFileDialogAndPickPath();
        if (result.Succeeded) generalDependencyContainer.SettingsService.Set("AudioPluginPath", result.Path, true);
    }

    [RelayCommand]
    private async void BrowseInputPluginPath()
    {
        var result = await ShowFileDialogAndPickPath();
        if (result.Succeeded) generalDependencyContainer.SettingsService.Set("InputPluginPath", result.Path, true);
    }

    [RelayCommand]
    private async void BrowseRspPluginPath()
    {
        var result = await ShowFileDialogAndPickPath();
        if (result.Succeeded) generalDependencyContainer.SettingsService.Set("RspPluginPath", result.Path, true);
    }
}