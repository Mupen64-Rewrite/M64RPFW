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
            FrameAdvanceCommand.NotifyCanExecuteChanged();
            PauseStateCommand.UpdateValue();
            PauseStateCommand.UpdateCanExecute();
        };

        _view.Closing += (_, _) =>
        {
            _vidext.NotifyClosing();
            if (IsNotStopped)
                CloseRom();
        };
        _view.Closed += (_, _) =>
        {
            AwaitThreadFinish();
        };
    }

    internal void PostInit()
    {
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

            Mupen64Plus.AttachPlugin(Settings.RPFW.Plugins.Video);
            Mupen64Plus.AttachPlugin(Settings.RPFW.Plugins.Audio);
            Mupen64Plus.AttachPlugin(Settings.RPFW.Plugins.Input);
            Mupen64Plus.AttachPlugin(Settings.RPFW.Plugins.RSP);

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
            
            // Set the savestate slot to be correct.
            var emulationMenu = (SubMenuItem) _view.Menu.Items[1];
            var savestateMenu = (SubMenuItem) emulationMenu.Items.First(item => item.ID == "save-slot-menu");

            int slot = Mupen64Plus.CoreStateQuery(Mupen64Plus.CoreParam.SavestateSlot);
            ((RadioMenuItem) savestateMenu.Items[slot]).Checked = true;
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

        var rom = new RomFile(fileDialog.FileName);
        _view.RomView.Presenter.RecentRoms.Add(rom);

        LaunchRom(rom);
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

    [RelayCommand(CanExecute = nameof(IsStopped))]
    public void ShowSettings()
    {
        var dialog = new SettingsView();
        dialog.ShowModal();
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

    #region PauseState custom command
    // I should write a source generator so it doesn't look so ugly

    private RelayValueCommand<bool>? _pauseStateCommand;

    public RelayValueCommand<bool> PauseStateCommand => _pauseStateCommand ??=
        new RelayValueCommand<bool>(() => PauseState, value => PauseState = value, () => {}, () => IsNotStopped);

    #endregion

    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void FrameAdvance()
    {
        if (IsRunning)
            Mupen64Plus.Pause();
        Mupen64Plus.AdvanceFrame();
    }
    
    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void LoadSlot()
    {
        Mupen64Plus.LoadStateFromCurrentSlot();
    }

    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void LoadSavestateFile()
    {
        OpenFileDialog fileDialog = new OpenFileDialog
        {
            Filters = { "Savestates|.st;.savestate;.pj" },
            MultiSelect = false
        };
        var result = fileDialog.ShowDialog(_view);
        if (result == DialogResult.Cancel)
            return;
        
        Mupen64Plus.LoadStateFromFile(fileDialog.FileName);
    }

    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void SaveSlot()
    {
        Mupen64Plus.SaveStateToCurrentSlot();
    }
    
    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void SaveSavestateFile()
    {
        SaveFileDialog fileDialog = new SaveFileDialog
        {
            Filters =
            {
                "M64+ Savestate|.st",
                "PJ64 Savestate|.pj",
                "PJ64 Savestate (uncompressed)|.pj"
            }
        };
        var result = fileDialog.ShowDialog(_view);
        if (result == DialogResult.Cancel)
            return;
        
        Mupen64Plus.SaveStateToFile(fileDialog.FileName, (Mupen64Plus.SavestateType) fileDialog.CurrentFilterIndex);
    }

    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void SetSavestateSlot(int slot)
    {
        Mupen64Plus.SetSavestateSlot(slot);
    }

    #endregion
}