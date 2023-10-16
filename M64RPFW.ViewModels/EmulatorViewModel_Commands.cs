using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Media.Encoder;
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

    private delegate void DTinCan_SetFrontendHandles(nint mainWinHandle, WindowSystemID mainWinSys);

    private void EmulatorThreadRun(object? romPathObj)
    {
        var (romPath, tcs, winHandle, winSystem) = ((string, TaskCompletionSource, nint, WindowSystemID)) romPathObj!;
        Mupen64Plus.OpenRom(romPath);
        
        try
        {
            Mupen64Plus.AttachPlugin(PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.VideoPath), PluginType.Graphics);
            Mupen64Plus.AttachPlugin(PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.AudioPath), PluginType.Audio);
            Mupen64Plus.AttachPlugin(PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.InputPath), PluginType.Input, handle =>
            {
                try
                {
                    var tinCanSetFrontendHandles = NativeLibHelper.GetFunction<DTinCan_SetFrontendHandles>(handle, "TinCan_SetFrontendHandles");
                    tinCanSetFrontendHandles?.Invoke(winHandle, winSystem);
                }
                catch (EntryPointNotFoundException)
                {
                    // ignored
                }
            });
            Mupen64Plus.AttachPlugin(PathHelper.DerefAppRelative(RPFWSettings.Instance.Plugins.RspPath), PluginType.RSP);
            tcs.SetResult();
        }
        catch (Exception e)
        {
            tcs.SetException(e);

            Mupen64Plus.CloseRom();

            Mupen64Plus.DetachPlugin(PluginType.Graphics);
            Mupen64Plus.DetachPlugin(PluginType.Audio);
            Mupen64Plus.DetachPlugin(PluginType.Input);
            Mupen64Plus.DetachPlugin(PluginType.RSP);
            return;
        }
        
        
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

        await OpenRomFromPath(files[0]);
    }

    [RelayCommand(CanExecute = nameof(MupenIsStopped))]
    private async Task OpenRomFromPath(string path)
    {
        WeakReferenceMessenger.Default.Send(new RomLoadingMessage(path));

        var loadTaskSource = new TaskCompletionSource();
        _emuThread = new Thread(EmulatorThreadRun);
        _emuThread.Start((path, loadTaskSource, _windowAccessService.WindowHandle, _windowAccessService.WindowSystemID));

        try
        {
            await loadTaskSource.Task;
        }
        catch (Exception e)
        {
            await _viewDialogService.ShowExceptionDialog(e, "Failed to load ROM");
        }
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
    private void SetSpeedLimiter(bool value)
    {
        Mupen64Plus.CoreStateSet(CoreParam.SpeedLimiter, value ? 1 : 0);
    }

    #endregion

    #region Encoder objects

    private FFmpegEncoder? _encoder;

    #endregion
    
    #region VCR/Encoder commands

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private async void StartMovie()
    {
        var result = await _viewDialogService.ShowOpenMovieDialog(false);

        if (result == null)
            return;
        // probably want to add a warning here
        if (!File.Exists(result.Path))
            return;
        
        StartMovieWithFile(result.Path);
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void StartMovieWithFile(string path)
    {
        if (Mupen64Plus.VCR_IsPlaying)
            Mupen64Plus.VCR_StopMovie();

        Mupen64Plus.VCR_StartMovie(path);
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

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private async void StartEncoder()
    {
        if (_encoder != null)
            return;
        
        var result = await _viewDialogService.ShowStartEncoderDialog();
        if (result == null)
            return;

        FFmpegConfig config = new FFmpegConfig();
        config.VideoOptions.Add("video_size", $"{result.EncodeSize ?? _frameCaptureService.GetWindowSize()}");

        await StartEncoderWithFile((result.Path, config));
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private async Task StartEncoderWithFile((string, FFmpegConfig?) args)
    {
        string path = args.Item1;
        FFmpegConfig config = args.Item2;
        Mupen64Plus.Log(LogSources.App, MessageLevel.Info, "Creating encoder...");
        try
        {
            _encoder = new FFmpegEncoder(path, null, config);
        }
        catch (ArgumentException e)
        {
            await _viewDialogService.ShowExceptionDialog(e);
            return;
        }
        Mupen64Plus.Log(LogSources.App, MessageLevel.Info, "Initializing audio hooks...");
        _encoder.SetAudioSampleRate((int) Mupen64Plus.GetSampleRate());
        unsafe
        {
            Mupen64Plus.AudioReceived += EncoderAudioReceived;
            Mupen64Plus.SampleRateChanged += EncoderSampleRateChanged;
            _frameCaptureService.OnRender += EncoderVideoReceived;
        }
        Mupen64Plus.Log(LogSources.App, MessageLevel.Info, "Starting encoder...");
        Mupen64Plus.Encoder_Start(path, null);
        Mupen64Plus.Log(LogSources.App, MessageLevel.Info, "Encoder initialized");
    }

    private void EncoderVideoReceived()
    {
        _encoder?.ConsumeVideo(_frameCaptureService);
    }

    private unsafe void EncoderAudioReceived(void* data, ulong len)
    {
        _encoder?.ConsumeAudio(data, len);
    }

    private void EncoderSampleRateChanged(uint rate)
    {
        _encoder?.SetAudioSampleRate((int) rate);
    }

    [RelayCommand(CanExecute = nameof(MupenIsActive))]
    private void StopEncoder()
    {
        if (_encoder == null)
            return;
        
        Mupen64Plus.Log(LogSources.App, MessageLevel.Info, "Stopping encoder...");
        Mupen64Plus.Encoder_Stop(false);

        Mupen64Plus.Log(LogSources.App, MessageLevel.Info, "Cleaning up audio hooks...");
        unsafe
        {
            Mupen64Plus.AudioReceived -= EncoderAudioReceived;
            Mupen64Plus.SampleRateChanged -= EncoderSampleRateChanged;
            _frameCaptureService.OnRender -= EncoderVideoReceived;
        }
        
        Mupen64Plus.Log(LogSources.App, MessageLevel.Info, "Closing file...");
        _encoder.Finish();
        _encoder.Dispose();
        Mupen64Plus.Log(LogSources.App, MessageLevel.Info, "Encoder shut down");

        _encoder = null;
    }

    #endregion

    #region View commands
    
    [RelayCommand]
    private async void ShowSettings()
    {
        await _viewDialogService.ShowSettingsDialog();
    }

    [RelayCommand]
    private async void ShowAdvancedSettings()
    {
        await _viewDialogService.ShowAdvancedSettingsDialog();
    }

    #endregion
}