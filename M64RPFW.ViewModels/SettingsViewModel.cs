using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Services;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Helpers;

namespace M64RPFW.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IFilesService _filesService;
    private readonly ILocalSettingsService _localSettingsService;

    #region Properties
    public List<string> RecentRomPaths
    {
        get => _localSettingsService.Get<List<string>>(nameof(RecentRomPaths));
        set => SetSettingsProperty(nameof(RecentRomPaths), value);
    }

    public string[] RomExtensions
    {
        get => _localSettingsService.Get<string[]>(nameof(RomExtensions));
        set => SetSettingsProperty(nameof(RomExtensions), value);
    }
    
    public string CoreLibraryPath
    {
        get => _localSettingsService.Get<string>(nameof(CoreLibraryPath));
        set => SetSettingsProperty(nameof(CoreLibraryPath), value);
    }
    
    public string VideoPluginPath
    {
        get => _localSettingsService.Get<string>(nameof(VideoPluginPath));
        set => SetSettingsProperty(nameof(VideoPluginPath), value);
    }
    
    public string AudioPluginPath
    {
        get => _localSettingsService.Get<string>(nameof(AudioPluginPath));
        set => SetSettingsProperty(nameof(AudioPluginPath), value);
    }
    
    public string InputPluginPath
    {
        get => _localSettingsService.Get<string>(nameof(InputPluginPath));
        set => SetSettingsProperty(nameof(InputPluginPath), value);
    }
    
    public string RspPluginPath
    {
        get => _localSettingsService.Get<string>(nameof(RspPluginPath));
        set => SetSettingsProperty(nameof(RspPluginPath), value);
    }
    
    public int CoreType
    {
        get => _localSettingsService.Get<int>(nameof(CoreType));
        set => SetSettingsProperty(nameof(CoreType), value);
    }
    
    public int ScreenWidth
    {
        get => _localSettingsService.Get<int>(nameof(ScreenWidth));
        set => SetSettingsProperty(nameof(ScreenWidth), value);
    }
    
    public int ScreenHeight
    {
        get => _localSettingsService.Get<int>(nameof(ScreenHeight));
        set => SetSettingsProperty(nameof(ScreenHeight), value);
    }

    public bool IsStatusbarVisible
    {
        get => _localSettingsService.Get<bool>(nameof(IsStatusbarVisible));
        set => SetSettingsProperty(nameof(IsStatusbarVisible), value);
    }
    
    public string Culture
    {
        get => _localSettingsService.Get<string>(nameof(Culture));
        set => SetSettingsProperty(nameof(Culture), value);
    }
    
    public string Theme
    {
        get => _localSettingsService.Get<string>(nameof(Theme));
        set => SetSettingsProperty(nameof(Theme), value);
    }
    
    #endregion

    private void SetSettingsProperty<T>(string key, T value, [CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(callerMemberName);
        _localSettingsService.Set(key, value);
        OnPropertyChanged(callerMemberName);
    }
    
    public SettingsViewModel(GeneralDependencyContainer generalDependencyContainer, ILocalSettingsService localSettingsService)
    {
        this._filesService = generalDependencyContainer.FilesService;
        _localSettingsService = localSettingsService;
    }

    private async Task<(bool Succeeded, string Path)> ShowLibraryFileDialog()
    {
        var file = await _filesService.TryPickOpenFileAsync(new[] { "dll" });
        return file != null ? (true, file.Path) : ((bool Succeeded, string Path))(false, null);
    }

    [RelayCommand]
    private async Task BrowseLibraryPath(string key)
    {
        var file = await _filesService.TryPickOpenFileAsync(new[] { "dll" });
        if (file != null)
        {
            _localSettingsService.Set(key, file.Path);
        }
        else
        {
            // we failed
            // TODO: maybe notify the user? not an important failure though
        }
    }
}