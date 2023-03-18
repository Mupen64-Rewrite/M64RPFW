using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Interfaces;
using M64RPFW.Services;
using M64RPFW.ViewModels.Interfaces;

namespace M64RPFW.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDispatcherService _dispatcherService;
    private readonly IVidextSurfaceService _vidextSurfaceService;
    private readonly IWindowDimensionsService _windowDimensionsService;
    private readonly IFilesService _filesService;
    
    public MainViewModel(IDispatcherService dispatcherService, IFilesService filesService,
        IVidextSurfaceService vidextSurfaceService, IWindowDimensionsService windowDimensionsService)
    {
        _dispatcherService = dispatcherService;
        _vidextSurfaceService = vidextSurfaceService;
        _windowDimensionsService = windowDimensionsService;
        _filesService = filesService;
        windowDimensionsService.DimensionsChanged += WindowDimensionsServiceOnDimensionsChanged;
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

    #region Tracker properties and events

    private Mupen64Plus.EmuState MupenEmuState =>
        (Mupen64Plus.EmuState)Mupen64Plus.CoreStateQuery(Mupen64Plus.CoreParam.EmuState);

    public bool IsMupenStopped => MupenEmuState is Mupen64Plus.EmuState.Stopped;
    public bool IsMupenRunning => MupenEmuState is Mupen64Plus.EmuState.Running;
    public bool IsMupenPaused => MupenEmuState is Mupen64Plus.EmuState.Paused;
    public bool IsMupenActive => MupenEmuState is Mupen64Plus.EmuState.Running or Mupen64Plus.EmuState.Paused;

    #endregion
}