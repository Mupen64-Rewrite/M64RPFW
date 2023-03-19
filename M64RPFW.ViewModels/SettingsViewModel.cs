using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Settings;
using M64RPFW.Models.Types;
using M64RPFW.Models.Types.Settings;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;


public sealed partial class SettingsViewModel : ObservableObject, IRecipient<RomLoadingMessage>
{
    
    public SettingsViewModel(IFilesService filesService, ILocalSettingsService localSettingsService)
    {
        _filesService = filesService;
        _localSettingsService = localSettingsService;
        // WeakReferenceMessenger.Default.RegisterAll(this);
    }
    
    private readonly IFilesService _filesService;
    private readonly ILocalSettingsService _localSettingsService;

    #region Properties
    public ObservableCollection<string> RecentRomPaths
    {
        get => _localSettingsService.Get<ObservableCollection<string>>(nameof(RecentRomPaths));
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
    
    public EmulatorType CoreType
    {
        get => Mupen64Plus.ConfigGet<EmulatorType>(MupenSettings.Core, "R4300Emulator");
        set => SetMupenSetting(MupenSettings.Core, "R4300Emulator", value);
    }
    
    public int ScreenWidth
    {
        get => Mupen64Plus.ConfigGet<int>(MupenSettings.VideoGeneral, "ScreenWidth");
        set => SetMupenSetting(MupenSettings.VideoGeneral, "ScreenWidth", value);
    }
    
    public int ScreenHeight
    {
        get => Mupen64Plus.ConfigGet<int>(MupenSettings.VideoGeneral, "ScreenHeight");
        set => SetMupenSetting(MupenSettings.VideoGeneral, "ScreenHeight", value);
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

    [RelayCommand]
    private void RemoveRecentRomPath(string path)
    {
        RecentRomPaths.Remove(path);
    } 

    void IRecipient<RomLoadingMessage>.Receive(RomLoadingMessage message)
    {
        RecentRomPaths.Remove(message.Value);
        RecentRomPaths.Insert(0, message.Value);
    }
    
    private void SetSettingsProperty<T>(string key, T value, [CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(callerMemberName);
        _localSettingsService.Set(key, value);
        OnPropertyChanged(callerMemberName);
    }

    private void SetMupenSetting<T>(IntPtr section, string key, T value, [CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(callerMemberName);
        Mupen64Plus.ConfigSet(section, key, value);
        OnPropertyChanged(callerMemberName);
    }
   

    private async Task<string?> ShowLibraryFileDialog()
    {
        var files = await _filesService.ShowOpenFilePickerAsync(options: new FilePickerOption[]
        {
            new($"Plugins ({NativeLibHelper.LibraryExtension})", Patterns: new[]
            {
                $"*{NativeLibHelper.LibraryExtension}"
            })
        });
        // return file != null ? (true, file.Path) : ((bool Succeeded, string Path))(false, null);
        return files != null && files.Length > 0 ? files[0].Path : null;
    }

    [RelayCommand]
    private async Task BrowseLibraryPath(string key)
    {
        var files = await _filesService.ShowOpenFilePickerAsync(options: new FilePickerOption[]
        {
            new($"Plugins ({NativeLibHelper.LibraryExtension})", Patterns: new[]
            {
                $"*{NativeLibHelper.LibraryExtension}"
            })
        });
        if (files != null && files.Length > 0)
        {
            _localSettingsService.Set(key, files[0].Path);
        }
        // notify user of cancelling?
    }

    
}