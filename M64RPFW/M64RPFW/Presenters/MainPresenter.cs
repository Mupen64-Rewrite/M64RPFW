using System;
using System.IO;
using System.Threading;
using CommunityToolkit.Mvvm.Input;
using Eto.Drawing;
using M64RPFW.Models;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Views;

namespace M64RPFW.Presenters;

internal partial class MainPresenter
{
    public MainPresenter(MainView view)
    {
        _emuThread = null;

        _vidextOverriden = false;
        _vidext = new VidextPresenter(view);
    }

    private void EmulatorThreadRun(object? param)
    {
        RomFile rom = (RomFile) param!;

        rom.LoadThisRom();

        Mupen64Plus.AttachPlugin("/usr/lib/mupen64plus/mupen64plus-video-rice.so");
        Mupen64Plus.AttachPlugin("/usr/lib/mupen64plus/mupen64plus-audio-sdl.so");
        Mupen64Plus.AttachPlugin("/usr/lib/mupen64plus/mupen64plus-input-sdl.so");
        Mupen64Plus.AttachPlugin("/usr/lib/mupen64plus/mupen64plus-rsp-hle.so");

        Mupen64Plus.Execute();

        Mupen64Plus.CloseRom();

        Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Graphics);
        Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Audio);
        Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Input);
        Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.RSP);
        
        Mupen64Plus.StateChanged += delegate(object? sender, Mupen64Plus.StateChangeEventArgs args)
        {
            if (args.Param == Mupen64Plus.CoreParam.EmuState)
                NotifyRunning();
        };
    }

    public void LaunchRom(RomFile rom)
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

    private Mupen64Plus.EmuState State =>
        (Mupen64Plus.EmuState) Mupen64Plus.CoreStateQuery(Mupen64Plus.CoreParam.EmuState);

    private bool IsRunning => State == Mupen64Plus.EmuState.Running;
    public bool IsNotStopped => State != Mupen64Plus.EmuState.Stopped;

    /// <summary>
    /// Notifies all commands dependent on the emulator's state.
    /// </summary>
    private void NotifyRunning()
    {
        StopCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(IsNotStopped))]
    public void Stop()
    {
        Mupen64Plus.Stop();
    }

    private Thread? _emuThread;

    private bool _vidextOverriden;
    private VidextPresenter _vidext;
}