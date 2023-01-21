using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Helpers;

namespace M64RPFW.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    public SettingsViewModel(GeneralDependencyContainer generalDependencyContainer)
    {
        this._generalDependencyContainer = generalDependencyContainer;
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
            _generalDependencyContainer.SettingsService.Set(vParams[0], bool.Parse(vParams[1]));
        else if (typeof(T) == typeof(int))
            _generalDependencyContainer.SettingsService.Set(vParams[0], int.Parse(vParams[1]));
        else if (typeof(T) == typeof(string))
            _generalDependencyContainer.SettingsService.Set(vParams[0], vParams[1]);
        else
            throw new Exception("Could not resolve VirtualArgument");
    }

    [RelayCommand]
    private void SetCulture(string culture)
    {
        _generalDependencyContainer.SettingsService.Set("Culture", culture, true);
    }

    [RelayCommand]
    private void SetTheme(string theme)
    {
        _generalDependencyContainer.SettingsService.Set("Theme", theme, true);
    }

    private async Task<(bool Succeeded, string Path)> ShowLibraryFileDialog()
    {
        var file = await _generalDependencyContainer.FilesService.TryPickOpenFileAsync(new[] { "dll" });
        return file != null ? (true, file.Path) : ((bool Succeeded, string Path))(false, null);
    }

    [RelayCommand]
    private async void BrowseLibraryPath(string key)
    {
        var file = await _generalDependencyContainer.FilesService.TryPickOpenFileAsync(new[] { "dll" });
        if (file != null)
        {
            _generalDependencyContainer.SettingsService.Set(key, file.Path, true);
        }
        else
        {
            // we failed
            // TODO: maybe notify the user? not an important failure though
        }
    }
}