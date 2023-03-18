using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Interfaces;
using M64RPFW.Models.Types;
using M64RPFW.Services;

namespace M64RPFW.ViewModels;

public partial class EmulatorViewModel : ObservableObject
{
    [ObservableProperty] private double _windowWidth = 640;
    [ObservableProperty] private double _windowHeight = 480;
    [ObservableProperty] private double _menuHeight;
    [ObservableProperty] private bool _resizable = true;
    
    private readonly IOpenGLContextService _openGlContextService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IFilesService _filesService;
    
    public EmulatorViewModel(IOpenGLContextService openGlContextService, IDispatcherService dispatcherService, IFilesService filesService)
    {
        _openGlContextService = openGlContextService;
        _dispatcherService = dispatcherService;
        _filesService = filesService;

        var version = Mupen64Plus.GetVersionInfo();
        Mupen64Plus.Log(Mupen64Plus.LogSources.App, Mupen64PlusTypes.MessageLevel.Info,
            $"Loaded M64+ v{version.VersionMajor}.{version.VersionMinor}.{version.VersionPatch}");

        // Register Mupen64Plus-related listeners
        Mupen64Plus.StateChanged += OnMupenStateChange;
        Mupen64Plus.OverrideVidExt(this.ToVidextStruct());
    }

    private void OnMupenStateChange(object? sender, Mupen64Plus.StateChangeEventArgs args)
    {
        switch (args.Param)
        {
            case Mupen64PlusTypes.CoreParam.EmuState:
                _dispatcherService.Execute(() =>
                {
                    OpenRomCommand.NotifyCanExecuteChanged();
                    CloseRomCommand.NotifyCanExecuteChanged();
                    ResetRomCommand.NotifyCanExecuteChanged();
                    PauseOrResumeCommand.NotifyCanExecuteChanged();
                    FrameAdvanceCommand.NotifyCanExecuteChanged();
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

    public void OnWindowClosed()
    {
        if (MupenIsActive)
            Mupen64Plus.Stop();
    }

    #region Tracker properties and events

    private Mupen64PlusTypes.EmuState MupenEmuState =>
        (Mupen64PlusTypes.EmuState)Mupen64Plus.CoreStateQuery(Mupen64PlusTypes.CoreParam.EmuState);

    public bool MupenIsStopped => MupenEmuState is Mupen64PlusTypes.EmuState.Stopped;
    public bool MupenIsRunning => MupenEmuState is Mupen64PlusTypes.EmuState.Running;
    public bool MupenIsPaused => MupenEmuState is Mupen64PlusTypes.EmuState.Paused;
    public bool MupenIsActive => MupenEmuState is Mupen64PlusTypes.EmuState.Running or Mupen64PlusTypes.EmuState.Paused;

    #endregion


}