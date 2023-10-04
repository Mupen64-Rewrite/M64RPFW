using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;
using M64RPFW.Services;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.ViewModels;

public partial class EmulatorViewModel : ObservableObject
{
    public static EmulatorViewModel? Instance { get; private set; }
    
    [ObservableProperty] private object? _currentSlotMenuItem;

    private readonly IOpenGLContextService _openGlContextService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IFilePickerService _filePickerService;
    private readonly IWindowAccessService _windowAccessService;
    private readonly IViewDialogService _viewDialogService;

    public EmulatorViewModel(IOpenGLContextService openGlContextService, IDispatcherService dispatcherService,
        IFilePickerService filePickerService, IWindowAccessService windowAccessService, IViewDialogService viewDialogService)
    {
        Instance = this;
        
        _openGlContextService = openGlContextService;
        _dispatcherService = dispatcherService;
        _filePickerService = filePickerService;
        _windowAccessService = windowAccessService;
        _viewDialogService = viewDialogService;

        var version = Mupen64Plus.GetVersionInfo();
        Mupen64Plus.Log(Mupen64Plus.LogSources.App, MessageLevel.Info,
            $"Loaded M64+ v{version.VersionMajor}.{version.VersionMinor}.{version.VersionPatch}");

        // Register Mupen64Plus-related listeners
        Mupen64Plus.StateChanged += OnMupenStateChange;
        Mupen64Plus.VCRStateChanged += OnVCRStateChange;
        Mupen64Plus.OverrideVidExt(this.ToVidextStruct());
    }

    private void OnMupenStateChange(Mupen64Plus.StateChangeEventArgs args)
    {
        switch (args.Param)
        {
            case CoreParam.EmuState:
                _dispatcherService.Execute(OnEmuStateChanged);
                break;
        }
    }

    private void OnVCRStateChange(Mupen64Plus.VCRStateChangeEventArgs args)
    {
        switch (args.Param)
        {
            case VCRParam.State:
                _dispatcherService.Execute(OnVCRIsPlayingChanged);
                break;
            case VCRParam.ReadOnly:
                _dispatcherService.Execute(OnVCRIsReadOnlyChanged);
                break;
        }
    }

    public void OnWindowClosed()
    {
        if (MupenIsActive)
            Mupen64Plus.Stop();
    }

    #region Tracker properties and events

    private EmuState MupenEmuState =>
        (EmuState) Mupen64Plus.CoreStateQuery(CoreParam.EmuState);

    private void OnEmuStateChanged()
    {
        OnPropertyChanged(nameof(MupenIsStopped));
        OnPropertyChanged(nameof(MupenIsPaused));
        OnPropertyChanged(nameof(MupenIsActive));

        OpenRomCommand.NotifyCanExecuteChanged();
        CloseRomCommand.NotifyCanExecuteChanged();
        ResetRomCommand.NotifyCanExecuteChanged();
        PauseOrResumeCommand.NotifyCanExecuteChanged();
        FrameAdvanceCommand.NotifyCanExecuteChanged();
        LoadFromFileCommand.NotifyCanExecuteChanged();
        SaveToFileCommand.NotifyCanExecuteChanged();
        SetSaveSlotCommand.NotifyCanExecuteChanged();
        LoadCurrentSlotCommand.NotifyCanExecuteChanged();
        SaveCurrentSlotCommand.NotifyCanExecuteChanged();
        StartMovieCommand.NotifyCanExecuteChanged();
        StartRecordingCommand.NotifyCanExecuteChanged();
    }

    public bool MupenIsStopped => MupenEmuState is EmuState.Stopped;
    public bool MupenIsPaused => MupenEmuState is EmuState.Paused;
    public bool MupenIsActive => MupenEmuState is EmuState.Running or EmuState.Paused;

    public bool VCRIsPlaying => Mupen64Plus.VCR_IsPlaying && MupenIsActive;
    public bool VCRDisableWrites => Mupen64Plus.VCR_DisableWrites;

    private void OnVCRIsPlayingChanged()
    {
        OnPropertyChanged(nameof(VCRIsPlaying));
        StopMovieCommand.NotifyCanExecuteChanged();
        RestartMovieCommand.NotifyCanExecuteChanged();
        ToggleDisableWritesCommand.NotifyCanExecuteChanged();
    }

    private void OnVCRIsReadOnlyChanged()
    {
        OnPropertyChanged(nameof(VCRDisableWrites));
    }

    #endregion
}