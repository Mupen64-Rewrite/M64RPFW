using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Emulation.API;
using M64RPFW.Services.Extensions;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Helpers;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public partial class EmulatorViewModel : ObservableObject, IRecipient<ApplicationExitingMessage>
{
    private readonly Emulator _emulator;
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    private int _saveStateSlot;

    internal EmulatorViewModel(GeneralDependencyContainer generalDependencyContainer)
    {
        this._generalDependencyContainer = generalDependencyContainer;

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
            Math.Clamp(value, 0, _generalDependencyContainer.SettingsService.Get<int>("SavestateSlots")));
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
        var file = await _generalDependencyContainer.FilesService.TryPickOpenFileAsync(_generalDependencyContainer
            .SettingsService.Get<string[]>("RomExtensions"));

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

        void ShowInvalidFileError() =>
            _generalDependencyContainer.DialogService.ShowError(
                _generalDependencyContainer.LocalizationService.GetString("InvalidFile"));

        if (!romViewModel.IsValid)
        {
            ShowInvalidFileError();
            return;
        }
        
        WeakReferenceMessenger.Default.Send(new RomLoadedMessage(romViewModel));
        

        if (IsRunning) _emulator.Stop();

        var coreLibraryFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _generalDependencyContainer.SettingsService.Get<string>("CoreLibraryPath"));
        var videoPluginFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _generalDependencyContainer.SettingsService.Get<string>("VideoPluginPath"));
        var audioPluginFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _generalDependencyContainer.SettingsService.Get<string>("AudioPluginPath"));
        var inputPluginFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _generalDependencyContainer.SettingsService.Get<string>("InputPluginPath"));
        var rspPluginFile =
            await _generalDependencyContainer.FilesService.GetFileFromPathAsync(
                _generalDependencyContainer.SettingsService.Get<string>("RspPluginPath"));

        Mupen64PlusConfig config = new(_generalDependencyContainer.SettingsService.Get<int>("CoreType"),
            _generalDependencyContainer.SettingsService.Get<int>("ScreenWidth"),
            _generalDependencyContainer.SettingsService.Get<int>("ScreenHeight"));
        Mupen64PlusLaunchParameters launchParameters = new(romViewModel.RawData,
            config,
            _generalDependencyContainer.SettingsService.Get<int>("DefaultSlot"),
            coreLibraryFile,
            videoPluginFile,
            audioPluginFile,
            inputPluginFile,
            rspPluginFile);
        _emulator.Start(launchParameters);

        _emulator.Api.OnFrameBufferCreate += delegate
        {
            _generalDependencyContainer.BitmapDrawingService.Create(_emulator.Api.BufferWidth,
                _emulator.Api.BufferHeight);
        };
        _emulator.Api.OnFrameBufferUpdate += delegate
        {
            _generalDependencyContainer.BitmapDrawingService.Draw(_emulator.Api.FrameBuffer, _emulator.Api.BufferWidth,
                _emulator.Api.BufferHeight);
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
        if (_generalDependencyContainer.SettingsService.Get<bool>("PauseOnFrameAdvance"))
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

        var coreLibraryExists = File.Exists(_generalDependencyContainer.SettingsService.Get<string>("CoreLibraryPath"));

        if (!Path.GetExtension(_generalDependencyContainer.SettingsService.Get<string>("CoreLibraryPath"))
                .Equals(".dll", StringComparison.InvariantCultureIgnoreCase)) coreLibraryExists = false;

        if (!coreLibraryExists)
            _generalDependencyContainer.DialogService.ShowError(
                _generalDependencyContainer.LocalizationService.GetString("CoreLibraryNotFound"));

        var videoPluginExists = File.Exists(_generalDependencyContainer.SettingsService.Get<string>("VideoPluginPath"));
        var audioPluginExists = File.Exists(_generalDependencyContainer.SettingsService.Get<string>("AudioPluginPath"));
        var inputPluginExists = File.Exists(_generalDependencyContainer.SettingsService.Get<string>("InputPluginPath"));
        var rspPluginExists = File.Exists(_generalDependencyContainer.SettingsService.Get<string>("RspPluginPath"));
        if (!videoPluginExists) missingPlugins.Add(_generalDependencyContainer.LocalizationService.GetString("Video"));

        if (!audioPluginExists) missingPlugins.Add(_generalDependencyContainer.LocalizationService.GetString("Audio"));

        if (!inputPluginExists) missingPlugins.Add(_generalDependencyContainer.LocalizationService.GetString("Input"));

        if (!rspPluginExists) missingPlugins.Add(_generalDependencyContainer.LocalizationService.GetString("Rsp"));

        if (!videoPluginExists || !audioPluginExists || !inputPluginExists || (!rspPluginExists && coreLibraryExists))
            _generalDependencyContainer.DialogService.ShowError(string.Format(
                _generalDependencyContainer.LocalizationService.GetString("PluginNotFoundSeries"),
                string.Join(", ", missingPlugins)));

        hasAllDependencies = coreLibraryExists && videoPluginExists && audioPluginExists && inputPluginExists &&
                             rspPluginExists;
    }

    public void Receive(ApplicationExitingMessage message)
    {
        // close the application 
        this.CloseRomCommand.ExecuteIfPossible(null);
    }
}