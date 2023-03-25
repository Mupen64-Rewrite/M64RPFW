﻿using System.Collections.ObjectModel;
using System.Linq.Expressions;
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
    
    public SettingsViewModel(IFilePickerService filePickerService)
    {
        _filePickerService = filePickerService;
        // WeakReferenceMessenger.Default.RegisterAll(this);
        RPFWSettings.Load();
    }
    
    private readonly IFilePickerService _filePickerService;

    #region Properties
    
    public string VideoPluginPath
    {
        get => PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.VideoPath);
        set => SetRPFWSetting((inst, val) => inst.Plugins.VideoPath = val, PathHelper.ResolveAppRelative(value));
    }
    
    public string AudioPluginPath
    {
        get => PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.AudioPath);
        set => SetRPFWSetting((inst, val) => inst.Plugins.AudioPath = val, PathHelper.ResolveAppRelative(value));
    }
    
    public string InputPluginPath
    {
        get => PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.InputPath);
        set => SetRPFWSetting((inst, val) => inst.Plugins.InputPath = val, PathHelper.ResolveAppRelative(value));
    }
    
    public string RspPluginPath
    {
        get => PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.RspPath);
        set => SetRPFWSetting((inst, val) => inst.Plugins.RspPath = val, PathHelper.ResolveAppRelative(value));
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

    public bool IsStatusBarVisible
    {
        get => RPFWSettings.Instance.General.IsStatusBarVisible;
        set => SetRPFWSetting((inst, val) => inst.General.IsStatusBarVisible = val, value);
    }
    
    public string Locale
    {
        get => RPFWSettings.Instance.General.Locale;
        set => SetRPFWSetting((inst, val) => inst.General.Locale = val, value);
    }
    
    // Save on dialog closing, otherwise it won't affect Mupen64Plus
    public void OnClosed()
    {
        Mupen64Plus.ConfigSaveFile();
        RPFWSettings.Save();
    }
    
    #endregion

    #region Constants

    public EmulatorType[] EmulatorTypes => Enum.GetValues<EmulatorType>();

    #endregion
    
    // [RelayCommand]
    // private void RemoveRecentRomPath(string path)
    // {
    //     RecentRomPaths.Remove(path);
    // } 
    //
    // void IRecipient<RomLoadingMessage>.Receive(RomLoadingMessage message)
    // {
    //     RecentRomPaths.Remove(message.Value);
    //     RecentRomPaths.Insert(0, message.Value);
    // }

    private void SetMupenSetting<T>(IntPtr section, string key, T value, [CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(callerMemberName);
        Mupen64Plus.ConfigSet(section, key, value);
        OnPropertyChanged(callerMemberName);
    }

    private void SetRPFWSetting<T>(Action<RPFWSettings, T> setter, T value, [CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(callerMemberName);
        setter(RPFWSettings.Instance, value);
        OnPropertyChanged(callerMemberName);
    }

    public void Receive(RomLoadingMessage message)
    {
        
    }
}