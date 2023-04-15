using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Settings;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Messages;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.ViewModels;

using PluginType = PluginType;
using LogSources = Mupen64Plus.LogSources;
using MessageLevel = MessageLevel;

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

        WeakReferenceMessenger.Default.Send(new RomLoadingMessage(files[0]));
        
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
    
    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private async void LoadFromFile()
    {
        var paths = await _filePickerService.ShowOpenFilePickerAsync(options: new FilePickerOption[]
        {
            new("Mupen64Plus save (.st, .savestate)", Patterns: new[] { "*.st", "*.savestate" }),
            new("Project64 save (.pj.zip)", Patterns: new[] { "*.pj.zip" }),
            new("Project64 uncompressed save (.pj)", Patterns: new[] { "*.pj" }),
        }, allowMultiple: false);
        if (paths == null)
            return;
        
        Mupen64Plus.LoadStateFromFile(paths[0]);
    }
    
    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private async void SaveToFile()
    {
        var path = await _filePickerService.ShowSaveFilePickerAsync(options: new FilePickerOption[]
        {
            new("Mupen64Plus save (.st)", Patterns: new[] { "*.st" }),
            new("Mupen64Plus save (.savestate)", Patterns: new[] { "*.savestate" }),
            new("Project64 save (.pj.zip)", Patterns: new[] { "*.pj.zip" }),
            new("Project64 uncompressed save (.pj)", Patterns: new[] { "*.pj" }),
        });
        if (path == null)
            return;
        
        // Process extensions (Avalonia doesn't tell us which option was picked)
        var saveType = path switch
        {
            _ when path.EndsWith(".st") || path.EndsWith(".savestate") => SavestateType.Mupen64Plus,
            _ when path.EndsWith(".pj.zip") => SavestateType.Project64Compressed,
            _ when path.EndsWith(".pj") => SavestateType.Project64Uncompressed,
            _ => SavestateType.Mupen64Plus
        };
        
        Mupen64Plus.SaveStateToFile(path, saveType);
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void LoadCurrentSlot()
    {
        Mupen64Plus.LoadStateFromCurrentSlot();
    }
    
    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void SaveCurrentSlot()
    {
        Mupen64Plus.SaveStateToCurrentSlot();
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void SetSaveSlot()
    {
        if (CurrentSlotMenuItem == null)
            return;
        Mupen64Plus.SetSavestateSlot((int) CurrentSlotMenuItem);
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private async void StartMovie()
    {
        var result = await _viewDialogService.ShowOpenMovieDialog(false);

        if (result == null)
            return;
        // probably want to add a warning here
        if (!File.Exists(result.Path))
            return;
        
        if (Mupen64Plus.VCR_IsPlaying)
            Mupen64Plus.VCR_StopMovie();

        Mupen64Plus.VCR_StartMovie(result.Path);
        Mupen64Plus.VCR_DisableWrites = true;
    }
    
    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private async void StartRecording()
    {
        var result = await _viewDialogService.ShowOpenMovieDialog(true);
        
        if (result == null)
            return;

        if (File.Exists(result.Path))
        {
            // Perhaps add message boxes to view dialog service?
            Mupen64Plus.Log(LogSources.App, MessageLevel.Warning, "Overwriting a file...");
        }
        
        if (Mupen64Plus.VCR_IsPlaying)
            Mupen64Plus.VCR_StopMovie();
        
        Mupen64Plus.VCR_StartRecording(result.Path, result.Authors, result.Description, result.StartType);
        Mupen64Plus.VCR_DisableWrites = false;
    }
    
    [RelayCommand(CanExecute = nameof(VCRIsPlaying))]
    private void StopMovie()
    {
        Mupen64Plus.VCR_StopMovie();
    }

    [RelayCommand(CanExecute = nameof(VCRIsPlaying))]
    private void RestartMovie()
    {
        Mupen64Plus.VCR_RestartMovie();
    }

    [RelayCommand(CanExecute = nameof(VCRIsPlaying))]
    private void ToggleDisableWrites()
    {
        // toggle the current readonly state
        Mupen64Plus.VCR_DisableWrites = !Mupen64Plus.VCR_DisableWrites;
    }

    #endregion

    #region View commands
    
    [RelayCommand]
    private async void ShowSettings()
    {
        await _viewDialogService.ShowSettingsDialog();
    }

    #endregion
}