using System.Collections.Generic;
using System.IO;
using System.Threading;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Mupen64PlusRR.Models.Emulation;
using Mupen64PlusRR.Models.Helpers;
namespace Mupen64PlusRR.ViewModels;
using PluginType = Mupen64Plus.PluginType;
using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64Plus.MessageLevel;

public partial class MainWindowViewModel
{
    #region Emulator thread

    private static void EmulatorThreadRun(object? romPathObj)
    {
        string romPath = (string) romPathObj!;
        string bundlePath = Mupen64Plus.GetBundledLibraryPath();
        Mupen64Plus.OpenRom(romPath);

        Mupen64Plus.AttachPlugin(Path.Join(bundlePath, NativeLibHelper.AsDLL("mupen64plus-video-rice")));
        Mupen64Plus.AttachPlugin(Path.Join(bundlePath, NativeLibHelper.AsDLL("mupen64plus-audio-sdl")));
        Mupen64Plus.AttachPlugin(Path.Join(bundlePath, NativeLibHelper.AsDLL("mupen64plus-input-sdl")));
        Mupen64Plus.AttachPlugin(Path.Join(bundlePath, NativeLibHelper.AsDLL("mupen64plus-rsp-hle")));

        Mupen64Plus.Execute();

        Mupen64Plus.CloseRom();

        Mupen64Plus.DetachPlugin(PluginType.Graphics);
        Mupen64Plus.DetachPlugin(PluginType.Audio);
        Mupen64Plus.DetachPlugin(PluginType.Input);
        Mupen64Plus.DetachPlugin(PluginType.RSP);
    }

    private Thread? _emuThread;

    #endregion

    #region Emulator commands

    [RelayCommand(CanExecute = nameof(MupenIsStopped))]
    private async void OpenRom()
    {
        var paths = await SystemDialogService.ShowOpenDialog("Choose a ROM...", new List<FileDialogFilter>
        {
            new()
            {
                Name = "N64 ROMs",
                Extensions = { "n64", "z64", "v64" }
            }
        });
        if (paths == null)
            return;

        _emuThread = new Thread(EmulatorThreadRun);
        _emuThread.Start(paths[0]);
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void CloseRom()
    {
        Mupen64Plus.Log(LogSources.App, MessageLevel.Info, "Stopping M64+");
        Mupen64Plus.Stop();
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void ResetRom()
    {
        Mupen64Plus.Reset();
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void PauseOrResume()
    {
        if (MupenIsPaused)
            Mupen64Plus.Resume();
        else
            Mupen64Plus.Pause();
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void FrameAdvance()
    {
        Mupen64Plus.AdvanceFrame();
    }

    #endregion

    #region View commands
    
    [RelayCommand]
    private void ShowSettings()
    {
        ViewDialogService.ShowSettingsDialog();
    }

    #endregion
}