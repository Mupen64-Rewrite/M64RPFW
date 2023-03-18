using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.ViewModels;
using PluginType = Mupen64PlusTypes.PluginType;
using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64PlusTypes.MessageLevel;

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
        var file = await _filesService.TryPickOpenFileAsync(new[]
        {
            "n64", "z64", "v64"
        });
        
        if (file == null)
            return;

        _emuThread = new Thread(EmulatorThreadRun);
        _emuThread.Start(file.Path);
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void CloseRom()
    {
        Mupen64Plus.Log(Mupen64Plus.LogSources.App, MessageLevel.Info, "Stopping M64+");
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
}