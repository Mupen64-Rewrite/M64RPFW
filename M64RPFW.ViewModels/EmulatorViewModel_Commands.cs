using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Settings;
using M64RPFW.Models.Types;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.ViewModels;

using PluginType = Mupen64PlusTypes.PluginType;
using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64PlusTypes.MessageLevel;

public partial class EmulatorViewModel
{
    #region Emulator thread

    private static void EmulatorThreadRun(object? romPathObj)
    {
        string romPath = (string) romPathObj!;
        string bundlePath = Mupen64Plus.GetBundledLibraryPath();
        Mupen64Plus.OpenRom(romPath);

        Mupen64Plus.AttachPlugin(PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.VideoPath));
        Mupen64Plus.AttachPlugin(PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.AudioPath));
        Mupen64Plus.AttachPlugin(PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.InputPath));
        Mupen64Plus.AttachPlugin(PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.RspPath));

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
        var files = await _filePickerService.ShowOpenFilePickerAsync(options: new FilePickerOption[]
        {
            new("N64 ROMs (.n64, .z64, .v64)", Patterns: new[] { "*.n64", "*.z64", "*.v64" }),
        }, allowMultiple: false);
        if (files == null)
            return;

        _emuThread = new Thread(EmulatorThreadRun);
        _emuThread.Start(files[0]);
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
}