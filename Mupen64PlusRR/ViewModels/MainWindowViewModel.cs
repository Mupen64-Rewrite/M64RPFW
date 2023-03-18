using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Mupen64PlusRR.Models.Emulation;
using Mupen64PlusRR.Models.Interfaces;
using Mupen64PlusRR.ViewModels.Interfaces;

namespace Mupen64PlusRR.ViewModels;

using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64Plus.MessageLevel;
using EmuState = Mupen64Plus.EmuState;
using CoreParam = Mupen64Plus.CoreParam;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private double _windowWidth = 640;
    [ObservableProperty] private double _windowHeight = 480;
    [ObservableProperty] private double _menuHeight;
    [ObservableProperty] private bool _resizable = true;

    public MainWindowViewModel()
    {
        var version = Mupen64Plus.GetVersionInfo();
        Mupen64Plus.Log(LogSources.App, MessageLevel.Info,
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
                Dispatcher.UIThread.Post(() =>
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

    private EmuState MupenEmuState => (EmuState) Mupen64Plus.CoreStateQuery(CoreParam.EmuState);

    public bool MupenIsStopped => MupenEmuState is EmuState.Stopped;
    public bool MupenIsRunning => MupenEmuState is EmuState.Running;
    public bool MupenIsPaused => MupenEmuState is EmuState.Paused;
    public bool MupenIsActive => MupenEmuState is EmuState.Running or EmuState.Paused;

    #endregion

    #region Service properties

    private ISystemDialogService? _systemDialogService;

    public ISystemDialogService SystemDialogService
    {
        set => _systemDialogService ??= value;
        private get => _systemDialogService ?? throw new NullReferenceException("No registered IIODialogService");
    }

    private IVidextSurfaceService? _vidextSurfaceService;

    public IVidextSurfaceService VidextSurfaceService
    {
        set => _vidextSurfaceService ??= value;
        private get => _vidextSurfaceService ?? throw new NullReferenceException("No registered IVidextSurfaceService");
    }

    private IViewDialogService? _viewDialogService;

    public IViewDialogService ViewDialogService
    {
        set => _viewDialogService ??= value;
        private get => _viewDialogService ?? throw new NullReferenceException("No registered IViewDialogService");
    }

    #endregion
}