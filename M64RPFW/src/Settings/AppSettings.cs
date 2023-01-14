using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace M64RPFW.src.Settings;

[INotifyPropertyChanged]
internal sealed partial class AppSettings
{
    #region Emulation

    [ObservableProperty] private int coreType;

    [ObservableProperty] private string coreLibraryPath = "m64p/mupen64plus.dll";

    [ObservableProperty] private string videoPluginPath = "m64p/mupen64plus-video-rice.dll";

    [ObservableProperty] private string audioPluginPath = "m64p/mupen64plus-audio-sdl.dll";

    [ObservableProperty] private string inputPluginPath = "m64p/mupen64plus-input-sdl.dll";

    [ObservableProperty] private string rspPluginPath = "m64p/mupen64plus-rsp-hle.dll";

    [ObservableProperty] private int defaultSlot;

    [ObservableProperty] private bool pauseOnFrameAdvance = true;

    [ObservableProperty] private string[] recentRomPaths = Array.Empty<string>();

    [ObservableProperty] private int screenWidth = 800;

    [ObservableProperty] private int screenHeight = 600;

    #endregion

    #region Appearance

    [ObservableProperty] private string culture = "en-US";

    [ObservableProperty] private bool showStatusbar = true;

    [ObservableProperty] private string theme = "Light";

    #endregion
}