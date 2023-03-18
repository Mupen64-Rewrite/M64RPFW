using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Services;

namespace M64RPFW.ViewModels;

public partial class MainViewModel : ObservableObject
{

    #region Emulator thread

    private static void EmulatorThreadRun(object? romPathObj)
    {
        var romPath = (string)romPathObj!;
        var bundlePath = Mupen64Plus.GetBundledLibraryPath();
        Mupen64Plus.OpenRom(romPath);

        Mupen64Plus.AttachPlugin(Path.Join(bundlePath, NativeLibHelper.AsDLL("mupen64plus-video-rice")));
        Mupen64Plus.AttachPlugin(Path.Join(bundlePath, NativeLibHelper.AsDLL("mupen64plus-audio-sdl")));
        Mupen64Plus.AttachPlugin(Path.Join(bundlePath, NativeLibHelper.AsDLL("mupen64plus-input-sdl")));
        Mupen64Plus.AttachPlugin(Path.Join(bundlePath, NativeLibHelper.AsDLL("mupen64plus-rsp-hle")));

        Mupen64Plus.Execute();

        Mupen64Plus.CloseRom();

        Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Graphics);
        Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Audio);
        Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Input);
        Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.RSP);
    }

    private Thread? _emulatorThread;

    #endregion

    #region Emulator commands

    [RelayCommand(CanExecute = nameof(IsMupenStopped))]
    private async Task OpenRom()
    {
        var path = await _filesService.TryPickOpenFileAsync(new[] { "rom", "n64", "z64" });

        if (path == null)
            return;

        _emulatorThread = new Thread(() =>
        {
            EmulatorThreadRun(path.Path);
        })
        {
            Name = "tEmulatorThread"
        };
        _emulatorThread.Start();
    }

    [RelayCommand(CanExecute = nameof(IsMupenActive))]
    private void CloseRom()
    {
        Mupen64Plus.Log(Mupen64Plus.LogSources.App, Mupen64Plus.MessageLevel.Info, "Stopping M64+");
        Mupen64Plus.Stop();
    }

    [RelayCommand(CanExecute = nameof(IsMupenActive))]
    private void ResetRom()
    {
        Mupen64Plus.Reset();
    }

    [RelayCommand(CanExecute = nameof(IsMupenActive))]
    private void PauseOrResume()
    {
        if (IsMupenPaused)
            Mupen64Plus.Resume();
        else
            Mupen64Plus.Pause();
    }

    [RelayCommand(CanExecute = nameof(IsMupenActive))]
    private void FrameAdvance()
    {
        Mupen64Plus.AdvanceFrame();
    }

    #endregion
}