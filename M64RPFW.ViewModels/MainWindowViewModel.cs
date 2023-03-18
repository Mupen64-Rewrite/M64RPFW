using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Interfaces;
using M64RPFW.Services;
using M64RPFW.ViewModels.Interfaces;

namespace M64RPFW.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private double _windowWidth = 640;
    [ObservableProperty] private double _windowHeight = 480;
    [ObservableProperty] private double _menuHeight;
    [ObservableProperty] private bool _resizable = true;


    private readonly IVidextSurfaceService _vidextSurfaceService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IFilesService _filesService;
    
    public MainWindowViewModel(IVidextSurfaceService vidextSurfaceService, IDispatcherService dispatcherService, IFilesService filesService)
    {
        _vidextSurfaceService = vidextSurfaceService;
        _dispatcherService = dispatcherService;
        _filesService = filesService;
        var version = Mupen64Plus.GetVersionInfo();
        Mupen64Plus.Log(Mupen64Plus.LogSources.App, Mupen64Plus.MessageLevel.Info,
            $"Loaded M64+ v{version.VersionMajor}.{version.VersionMinor}.{version.VersionPatch}");

        // Register Mupen64Plus-related listeners
        Mupen64Plus.StateChanged += OnMupenStateChange;
        Mupen64Plus.OverrideVidExt(this.ToVidextStruct());
    }

    private void OnMupenStateChange(object? sender, Mupen64Plus.StateChangeEventArgs args)
    {
        switch (args.Param)
        {
            case Mupen64Plus.CoreParam.EmuState:
                _dispatcherService.Execute(() =>
                {
                    OpenRomCommand.NotifyCanExecuteChanged();
                    CloseRomCommand.NotifyCanExecuteChanged();
                });
                break;
        }
    }

    partial void OnSizeChanged();

    partial void OnWindowWidthChanged(double value)
    {
        OnSizeChanged();
    }
    
    partial void OnWindowHeightChanged(double value)
    {
        OnSizeChanged();
    }

    #region Tracker properties and events

    private Mupen64Plus.EmuState MupenEmuState =>
        (Mupen64Plus.EmuState)Mupen64Plus.CoreStateQuery(Mupen64Plus.CoreParam.EmuState);

    public bool MupenIsStopped => MupenEmuState is Mupen64Plus.EmuState.Stopped;
    public bool MupenIsRunning => MupenEmuState is Mupen64Plus.EmuState.Running;
    public bool MupenIsPaused => MupenEmuState is Mupen64Plus.EmuState.Paused;
    public bool MupenIsActive => MupenEmuState is Mupen64Plus.EmuState.Running or Mupen64Plus.EmuState.Paused;

    #endregion


}