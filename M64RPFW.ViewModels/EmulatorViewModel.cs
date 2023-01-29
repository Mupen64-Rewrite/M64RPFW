using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Emulation.API;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.Services.Extensions;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Helpers;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public sealed partial class EmulatorViewModel : ObservableObject, IRecipient<ApplicationClosingMessage>
{
    private readonly Emulator _emulator;
    private readonly GeneralDependencyContainer _generalDependencyContainer;
    private readonly SettingsViewModel _settingsViewModel;

    private IBitmap? _bitmap = null;

    private int _saveStateSlot;

    internal EmulatorViewModel(GeneralDependencyContainer generalDependencyContainer,
        SettingsViewModel settingsViewModel)
    {
        _generalDependencyContainer = generalDependencyContainer;
        _settingsViewModel = settingsViewModel;

        _emulator = new Emulator(generalDependencyContainer.FilesService);
        _emulator.PlayModeChanged += PlayModeChanged;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public bool IsRunning => _emulator.PlayMode != Mupen64PlusTypes.PlayModes.Stopped;

    public bool IsResumed
    {
        get => _emulator.PlayMode == Mupen64PlusTypes.PlayModes.Running;
        set => _emulator.PlayMode = value ? Mupen64PlusTypes.PlayModes.Running : Mupen64PlusTypes.PlayModes.Paused;
    }

    public int SaveStateSlot
    {
        get => _saveStateSlot;
        set => SetProperty(ref _saveStateSlot,
            Math.Clamp(value, 0, 10));
    }

    private void PlayModeChanged()
    {
        _generalDependencyContainer.DispatcherService.Execute(() =>
        {
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(IsResumed));
            CommandHelper.NotifyCanExecuteChanged(CloseRomCommand, ResetRomCommand, FrameAdvanceCommand,
                TogglePauseCommand);
        });
    }


    [RelayCommand]
    private async void BrowseRom()
    {
        var file =
            await _generalDependencyContainer.FilesService.TryPickOpenFileAsync(_settingsViewModel.RomExtensions);

        if (file != null)
        {
            var bytes = await file.ReadAllBytes();
            LoadRomCommand.ExecuteIfPossible(new RomViewModel(bytes, file.Path));
        }
    }

    [RelayCommand]
    private async void LoadRom(RomViewModel romViewModel)
    {
        TryVerifyDependencies(out var hasAllDependencies);

        if (!hasAllDependencies) return;

        void ShowInvalidFileError()
        {
            _generalDependencyContainer.DialogService.ShowError(
                _generalDependencyContainer.LocalizationService.GetStringOrDefault("InvalidFile"));
        }

        if (!romViewModel.IsValid)
        {
            ShowInvalidFileError();
            return;
        }

        WeakReferenceMessenger.Default.Send(new RomLoadedMessage(romViewModel));

        if (IsRunning) _emulator.Stop();

        var coreLibraryFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _settingsViewModel.CoreLibraryPath);
        var videoPluginFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _settingsViewModel.VideoPluginPath);
        var audioPluginFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _settingsViewModel.AudioPluginPath);
        var inputPluginFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _settingsViewModel.InputPluginPath);
        var rspPluginFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _settingsViewModel.RspPluginPath);

        Mupen64PlusConfig config = new(_settingsViewModel.CoreType,
            _settingsViewModel.ScreenWidth,
            _settingsViewModel.ScreenHeight);

        Mupen64PlusLaunchParameters launchParameters = new(romViewModel.RawData,
            config,
            0,
            coreLibraryFile,
            videoPluginFile,
            audioPluginFile,
            inputPluginFile,
            rspPluginFile);

        _emulator.Start(launchParameters);

        // _generalDependencyContainer.DispatcherService.Execute(() =>
        // {
        //     _bitmap = _generalDependencyContainer.BitmapsService.Create(6,
        //         6);
        //     _bitmap.Draw(Enumerable.Repeat(0xFF0000, 6 * 6).ToArray(), 6,
        //         6);
        // });
        
        _emulator.Api.OnFrameBufferCreate += delegate
        {
            _generalDependencyContainer.DispatcherService.Execute(() =>
                {
                    _bitmap ??= _generalDependencyContainer.BitmapsService.Create(IBitmapsService.BitmapTargets.Game, _emulator.Api.BufferWidth,
                        _emulator.Api.BufferHeight);
                }
            );
        };
        _emulator.Api.OnFrameBufferUpdate += delegate
        {
            _generalDependencyContainer.DispatcherService.Execute(() =>
            {
                _bitmap.Draw(_emulator.Api.FrameBuffer, _emulator.Api.BufferWidth,
                    _emulator.Api.BufferHeight);
            });
        };
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void CloseRom()
    {
        _emulator.Stop();
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void ResetRom()
    {
        _emulator.Reset();
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void FrameAdvance()
    {
        _emulator.PlayMode = Mupen64PlusTypes.PlayModes.Paused;

        _emulator.Api.FrameAdvance();
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void TogglePause()
    {
        _emulator.PlayMode = _emulator.PlayMode == Mupen64PlusTypes.PlayModes.Running
            ? Mupen64PlusTypes.PlayModes.Paused
            : Mupen64PlusTypes.PlayModes.Running;
    }

    private void TryVerifyDependencies(out bool hasAllDependencies)
    {
        // TODO: clean this up

        List<string> missingPlugins = new();

        var coreLibraryExists = File.Exists(_settingsViewModel.CoreLibraryPath);

        if (!Path.GetExtension(_settingsViewModel.CoreLibraryPath)
                .Equals(".dll", StringComparison.InvariantCultureIgnoreCase)) coreLibraryExists = false;

        if (!coreLibraryExists)
            _generalDependencyContainer.DialogService.ShowError(
                _generalDependencyContainer.LocalizationService.GetStringOrDefault("CoreLibraryNotFound"));

        var videoPluginExists = File.Exists(_settingsViewModel.VideoPluginPath);
        var audioPluginExists = File.Exists(_settingsViewModel.AudioPluginPath);
        var inputPluginExists = File.Exists(_settingsViewModel.InputPluginPath);
        var rspPluginExists = File.Exists(_settingsViewModel.RspPluginPath);
        if (!videoPluginExists)
            missingPlugins.Add(_generalDependencyContainer.LocalizationService.GetStringOrDefault("Video"));

        if (!audioPluginExists)
            missingPlugins.Add(_generalDependencyContainer.LocalizationService.GetStringOrDefault("Audio"));

        if (!inputPluginExists)
            missingPlugins.Add(_generalDependencyContainer.LocalizationService.GetStringOrDefault("Input"));

        if (!rspPluginExists)
            missingPlugins.Add(_generalDependencyContainer.LocalizationService.GetStringOrDefault("Rsp"));

        if (!videoPluginExists || !audioPluginExists || !inputPluginExists || (!rspPluginExists && coreLibraryExists))
            _generalDependencyContainer.DialogService.ShowError(string.Format(
                _generalDependencyContainer.LocalizationService.GetStringOrDefault("PluginNotFoundSeries"),
                string.Join(", ", missingPlugins)));

        hasAllDependencies = coreLibraryExists && videoPluginExists && audioPluginExists && inputPluginExists &&
                             rspPluginExists;
    }

    public void Receive(ApplicationClosingMessage message)
    {
        // close the application 
        this.CloseRomCommand.ExecuteIfPossible(null);
    }
}