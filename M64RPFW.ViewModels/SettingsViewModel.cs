using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Settings;
using M64RPFW.Models.Types.Settings;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Helpers;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject, IRecipient<RomLoadingMessage>
{
    private SettingsViewModel()
    {
        RPFWSettings.Load();
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    static SettingsViewModel()
    {
        Instance = new SettingsViewModel();
    }

    public static SettingsViewModel Instance { get; }

    #region Properties

    public string Culture
    {
        get => RPFWSettings.Instance.View.Culture;
        set => SetRPFWSetting((inst, val) => inst.View.Culture = val, value);
    }
    
    public string Theme
    {
        get => RPFWSettings.Instance.View.Theme;
        set => SetRPFWSetting((inst, val) => inst.View.Theme = val, value);
    }
    
    public ObservableCollection<string> RecentRoms
    {
        get => RPFWSettings.Instance.View.RecentRoms;
        set => SetRPFWSetting((inst, val) => inst.View.RecentRoms = val, value);
    }
    
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
        get => RPFWSettings.Instance.View.IsStatusBarVisible;
        set => SetRPFWSetting((inst, val) => inst.View.IsStatusBarVisible = val, value);
    }

    public string OpenRomHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.OpenRom;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.OpenRom = val, value);
    }

    public string CloseRomHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.CloseRom;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.CloseRom = val, value);
    }

    public string ResetRomHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.ResetRom;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.ResetRom = val, value);
    }

    public string PauseOrResumeHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.PauseOrResume;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.PauseOrResume = val, value);
    }

    public string FrameAdvanceHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.FrameAdvance;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.FrameAdvance = val, value);
    }

    public string LoadFromFileHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.LoadFromFile;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.LoadFromFile = val, value);
    }

    public string SaveToFileHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.SaveToFile;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.SaveToFile = val, value);
    }

    public string LoadCurrentSlotHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.LoadCurrentSlot;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.LoadCurrentSlot = val, value);
    }

    public string SaveCurrentSlotHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.SaveCurrentSlot;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.SaveCurrentSlot = val, value);
    }

    public string StartMovieHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.StartMovie;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.StartMovie = val, value);
    }

    public string StartRecordingHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.StartRecording;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.StartRecording = val, value);
    }

    public string StopMovieHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.StopMovie;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.StopMovie = val, value);
    }

    public string RestartMovieHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.RestartMovie;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.RestartMovie = val, value);
    }
    
    public string DisableWritesHotkey
    {
        get => RPFWSettings.Instance.Hotkeys.DisableWrites;
        set => SetRPFWSetting((inst, val) => inst.Hotkeys.DisableWrites = val, value);
    }

    [RelayCommand]
    private void Save()
    {
        Mupen64Plus.ConfigSaveFile();
        RPFWSettings.Save();
    }

    #endregion

    #region Constants

    public EmulatorType[] EmulatorTypes => Enum.GetValues<EmulatorType>();

    public FilePickerOption[] DllPickerOptions => new FilePickerOption[]
    {
        new($"Dynamic library ({NativeLibHelper.LibraryExtension})",
            Patterns: new[] { $"*{NativeLibHelper.LibraryExtension}" })
    };

    #endregion


    private void SetMupenSetting<T>(IntPtr section, string key, T value,
        [CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(callerMemberName);
        Mupen64Plus.ConfigSet(section, key, value);
        OnPropertyChanged(callerMemberName);
    }

    private void SetRPFWSetting<T>(Action<RPFWSettings, T> setter, T value,
        [CallerMemberName] string? callerMemberName = null)
    {
        ArgumentNullException.ThrowIfNull(callerMemberName);
        setter(RPFWSettings.Instance, value);
        OnPropertyChanged(callerMemberName);
    }

    public void Receive(RomLoadingMessage message)
    {
        RecentRoms.Add(message.Value);
    }

    // HACK: notify all properties changed on this class
    public void NotifyAllPropertiesChanged()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var propertyInfo in properties)
        {
            OnPropertyChanged(propertyInfo.Name);
        }
    }
}