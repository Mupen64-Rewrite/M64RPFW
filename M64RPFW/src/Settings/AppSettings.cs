using System;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace M64RPFW.src.Settings;

internal sealed partial class AppSettings : ObservableObject
{

    public void NotifyAllPropertiesChanged()
    {
        foreach (var property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            OnPropertyChanged(property.Name);
        }
    }
    
    #region Emulation

    [ObservableProperty] private int _coreType;

    [ObservableProperty] private string _coreLibraryPath = "m64p/mupen64plus.dll";

    [ObservableProperty] private string _videoPluginPath = "m64p/mupen64plus-video-rice.dll";

    [ObservableProperty] private string _audioPluginPath = "m64p/mupen64plus-audio-sdl.dll";

    [ObservableProperty] private string _inputPluginPath = "m64p/mupen64plus-input-sdl.dll";

    [ObservableProperty] private string _rspPluginPath = "m64p/mupen64plus-rsp-hle.dll";

    [ObservableProperty] private int _defaultSlot;

    [ObservableProperty] private bool _pauseOnFrameAdvance = true;

    [ObservableProperty] private string[] _recentRomPaths = Array.Empty<string>();

    [ObservableProperty] private int _screenWidth = 800;

    [ObservableProperty] private int _screenHeight = 600;

    [ObservableProperty] private int _savestateSlotCount = 10;
    
    [ObservableProperty] private string[] _romExtensions = new[] { "n64", "z64", "rom", "eu", "usa" };

    #endregion

    #region Appearance

    [ObservableProperty] private string _culture = "en-US";

    [ObservableProperty] private bool _showStatusbar = true;

    [ObservableProperty] private string _theme = "Light";

    #endregion
}