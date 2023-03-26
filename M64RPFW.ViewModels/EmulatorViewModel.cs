using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Interfaces;
using M64RPFW.Services;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.ViewModels;

public partial class EmulatorViewModel : ObservableObject
{
    [ObservableProperty] private double _windowWidth = 640;
    [ObservableProperty] private double _windowHeight = 480;
    [ObservableProperty] private double _menuHeight;
    [ObservableProperty] private bool _resizable = true;
    [ObservableProperty] private object? _currentSlotMenuItem;
    
    private readonly IOpenGLContextService _openGlContextService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IFilePickerService _filePickerService;
    
    public EmulatorViewModel(IOpenGLContextService openGlContextService, IDispatcherService dispatcherService, IFilePickerService filePickerService)
    {
        _openGlContextService = openGlContextService;
        _dispatcherService = dispatcherService;
        _filePickerService = filePickerService;

        var version = Mupen64Plus.GetVersionInfo();
        Mupen64Plus.Log(Mupen64Plus.LogSources.App, MessageLevel.Info,
            $"Loaded M64+ v{version.VersionMajor}.{version.VersionMinor}.{version.VersionPatch}");

        // Register Mupen64Plus-related listeners
        Mupen64Plus.StateChanged += OnMupenStateChange;
        Mupen64Plus.OverrideVidExt(this.ToVidextStruct());
    }

    private void OnMupenStateChange(object? sender, Mupen64Plus.StateChangeEventArgs args)
    {
        switch (args.Param)
        {
            case CoreParam.EmuState:
                _dispatcherService.Execute(OnEmuStateChanged);
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

    private EmuState MupenEmuState =>
        (EmuState)Mupen64Plus.CoreStateQuery(CoreParam.EmuState);

    private void OnEmuStateChanged()
    {
        OpenRomCommand.NotifyCanExecuteChanged();
        CloseRomCommand.NotifyCanExecuteChanged();
        ResetRomCommand.NotifyCanExecuteChanged();
        PauseOrResumeCommand.NotifyCanExecuteChanged();
        FrameAdvanceCommand.NotifyCanExecuteChanged();
        SetSaveSlotCommand.NotifyCanExecuteChanged();
    }

    public bool MupenIsStopped => MupenEmuState is EmuState.Stopped;
    public bool MupenIsRunning => MupenEmuState is EmuState.Running;
    public bool MupenIsPaused => MupenEmuState is EmuState.Paused;
    public bool MupenIsActive => MupenEmuState is EmuState.Running or EmuState.Paused;
    
    

    #endregion
}