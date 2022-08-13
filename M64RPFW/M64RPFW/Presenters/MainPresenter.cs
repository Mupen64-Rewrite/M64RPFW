using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CommunityToolkit.Mvvm.Input;
using Eto.Forms;
using M64RPFW.Models;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Models.Settings;
using M64RPFW.Views;

namespace M64RPFW.Presenters;

internal partial class MainPresenter
{
    /// <summary>
    /// List of properties dependent on emulator state.
    /// </summary>
    private static readonly string[] StateProperties;

    static MainPresenter()
    {
        StateProperties = new[]
        {
            nameof(IsRunning),
            nameof(IsPaused),
            nameof(IsStopped),
            nameof(IsStopped)
        };
    }

    public MainPresenter(MainView view)
    {
        _view = view;

        _emuThread = null;

        _vidextOverriden = false;
        _vidext = new VidextPresenter(view);

        Mupen64Plus.StateChanged += delegate(object? _, Mupen64Plus.StateChangeEventArgs args)
        {
            if (args.Param == Mupen64Plus.CoreParam.EmuState)
            {
                EmuStateChanged?.Invoke(this, EventArgs.Empty);
            }
        };

        EmuStateChanged += (_, _) =>
        {
            OpenRomCommand.NotifyCanExecuteChanged();
            CloseRomCommand.NotifyCanExecuteChanged();
            ResetCommand.NotifyCanExecuteChanged();
        };

        _view.Closing += (_, _) =>
        {
            _vidext.NotifyClosing();
            if (IsNotStopped)
                CloseRom();
        };
        _view.Closed += (_, _) =>
        {
            if (_emuThread is null)
                return;
            if (_emuThread.IsAlive)
                _emuThread.Join();
        };
    }

    #region Emulator launcher

    private MainView _view;

    private Thread? _emuThread;

    private bool _vidextOverriden;
    private VidextPresenter _vidext;

    private void EmulatorThreadRun(object? param)
    {
        try
        {
            RomFile rom = (RomFile) param!;

            rom.LoadThisRom();

            Mupen64Plus.AttachPlugin(Path.Join(Settings.Model.Plugins.SearchDir, Settings.Model.Plugins.Video));
            Mupen64Plus.AttachPlugin(Path.Join(Settings.Model.Plugins.SearchDir, Settings.Model.Plugins.Audio));
            Mupen64Plus.AttachPlugin(Path.Join(Settings.Model.Plugins.SearchDir, Settings.Model.Plugins.Input));
            Mupen64Plus.AttachPlugin(Path.Join(Settings.Model.Plugins.SearchDir, Settings.Model.Plugins.RSP));

            Mupen64Plus.Execute();
        }
        finally
        {
            if (!IsStopped)
                Mupen64Plus.Stop();
            
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Graphics);
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Audio);
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Input);
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.RSP);
        
            Mupen64Plus.CloseRom();
        }
    }

    internal void LaunchRom(RomFile rom)
    {
        if (!_vidextOverriden)
        {
            Mupen64Plus.OverrideVideoExtension(_vidext);
            _vidextOverriden = true;
        }

        if (State == Mupen64Plus.EmuState.Stopped)
        {
            _emuThread = new Thread(EmulatorThreadRun);
            _emuThread.Start(rom);
        }
    }

    public void AwaitThreadFinish()
    {
        if (_emuThread is { IsAlive: true })
            _emuThread.Join();
    }

    #endregion

    private Mupen64Plus.EmuState State =>
        (Mupen64Plus.EmuState) Mupen64Plus.CoreStateQuery(Mupen64Plus.CoreParam.EmuState);

    public bool IsRunning => State == Mupen64Plus.EmuState.Running;
    public bool IsPaused => State == Mupen64Plus.EmuState.Paused;
    public bool IsStopped => State == Mupen64Plus.EmuState.Stopped;
    public bool IsNotStopped => State != Mupen64Plus.EmuState.Stopped;

    public event EventHandler EmuStateChanged; 

    #region File menu

    [RelayCommand(CanExecute = nameof(IsStopped))]
    public void OpenRom()
    {
        FileFilter romFilter = new FileFilter("ROM Files", ".z64", ".n64", ".v64");
        OpenFileDialog fileDialog = new OpenFileDialog
        {
            MultiSelect = false,
            Filters = { romFilter }
        };
        var result = fileDialog.ShowDialog(_view);
        if (result is DialogResult.Cancel)
            return;

        LaunchRom(new RomFile(fileDialog.Filenames.First()));
    }

    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void CloseRom()
    {
        Mupen64Plus.Stop();
    }

    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void Reset()
    {
        Mupen64Plus.Reset();
    }

    #endregion

    #region Emulation menu
    
    public bool PauseState
    {
        get => IsPaused;
        set
        {
            if (value)
                Mupen64Plus.Pause();
            else
                Mupen64Plus.Resume();
        }
    }
    
    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void FrameAdvance()
    {
        if (IsRunning)
            Mupen64Plus.Pause();
        Mupen64Plus.AdvanceFrame();
    }
    
    
    #endregion
}