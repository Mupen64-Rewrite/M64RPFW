using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Emulation.API;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Helpers;
using M64RPFW.ViewModels.Interfaces;

namespace M64RPFW.ViewModels;

public partial class EmulatorViewModel : ObservableObject
{
    private readonly Emulator emulator;
    private readonly GeneralDependencyContainer generalDependencyContainer;
    private readonly IRecentRomsProvider recentRomsProvider;

    private int saveStateSlot;

    internal EmulatorViewModel(GeneralDependencyContainer generalDependencyContainer,
        IAppExitEventProvider appExitEventProvider, IRecentRomsProvider recentRomsProvider)
    {
        this.generalDependencyContainer = generalDependencyContainer;
        this.recentRomsProvider = recentRomsProvider;

        appExitEventProvider.Register(delegate { CloseRomCommand.ExecuteIfPossible(); });

        emulator = new Emulator(generalDependencyContainer.FilesService);
        emulator.PlayModeChanged += PlayModeChanged;
    }

    public bool IsRunning => emulator.PlayMode != Mupen64PlusTypes.PlayModes.Stopped;

    public bool IsResumed
    {
        get => emulator.PlayMode == Mupen64PlusTypes.PlayModes.Running;
        set => emulator.PlayMode = value ? Mupen64PlusTypes.PlayModes.Running : Mupen64PlusTypes.PlayModes.Paused;
    }

    public int SaveStateSlot
    {
        get => saveStateSlot;
        set => SetProperty(ref saveStateSlot,
            Math.Clamp(value, 0, generalDependencyContainer.SavestateBoundsConfiguration.Slots));
    }

    private void PlayModeChanged()
    {
        generalDependencyContainer.DispatcherService.Execute(() =>
        {
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(IsResumed));
            ICommandHelper.NotifyCanExecuteChanged(CloseRomCommand, ResetRomCommand, FrameAdvanceCommand,
                TogglePauseCommand);
        });
    }


    [RelayCommand]
    private async void BrowseRom()
    {
        var file = await generalDependencyContainer.FilesService.TryPickOpenFileAsync(generalDependencyContainer
            .RomFileExtensionsConfiguration.RomExtensions);

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
            generalDependencyContainer.DialogService.ShowError(
                generalDependencyContainer.LocalizationService.GetString("InvalidFile"));
        }

        if (!romViewModel.IsValid)
        {
            ShowInvalidFileError();
            return;
        }

        recentRomsProvider.Add(romViewModel);

        if (IsRunning) emulator.Stop();

        var coreLibraryFile =
            await generalDependencyContainer.FilesService.GetFileFromPathAsync(
                generalDependencyContainer.SettingsService.Get<string>("CoreLibraryPath"));
        var videoPluginFile =
            await generalDependencyContainer.FilesService.GetFileFromPathAsync(
                generalDependencyContainer.SettingsService.Get<string>("VideoPluginPath"));
        var audioPluginFile =
            await generalDependencyContainer.FilesService.GetFileFromPathAsync(
                generalDependencyContainer.SettingsService.Get<string>("AudioPluginPath"));
        var inputPluginFile =
            await generalDependencyContainer.FilesService.GetFileFromPathAsync(
                generalDependencyContainer.SettingsService.Get<string>("InputPluginPath"));
        var rspPluginFile =
            await generalDependencyContainer.FilesService.GetFileFromPathAsync(
                generalDependencyContainer.SettingsService.Get<string>("RspPluginPath"));

        Mupen64PlusConfig config = new(generalDependencyContainer.SettingsService.Get<int>("CoreType"),
            generalDependencyContainer.SettingsService.Get<int>("ScreenWidth"),
            generalDependencyContainer.SettingsService.Get<int>("ScreenHeight"));
        Mupen64PlusLaunchParameters launchParameters = new(romViewModel.RawData,
            config,
            generalDependencyContainer.SettingsService.Get<int>("DefaultSlot"),
            coreLibraryFile,
            videoPluginFile,
            audioPluginFile,
            inputPluginFile,
            rspPluginFile);
        emulator.Start(launchParameters);

        emulator.API.OnFrameBufferCreate += delegate
        {
            generalDependencyContainer.BitmapDrawingService.Create(emulator.API.BufferWidth,
                emulator.API.BufferHeight);
        };
        emulator.API.OnFrameBufferUpdate += delegate
        {
            generalDependencyContainer.BitmapDrawingService.Draw(emulator.API.FrameBuffer, emulator.API.BufferWidth,
                emulator.API.BufferHeight);
        };
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void CloseRom()
    {
        emulator.Stop();
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void ResetRom()
    {
        emulator.Reset();
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void FrameAdvance()
    {
        if (generalDependencyContainer.SettingsService.Get<bool>("PauseOnFrameAdvance"))
            emulator.PlayMode = Mupen64PlusTypes.PlayModes.Paused;

        emulator.API.FrameAdvance();
    }

    [RelayCommand(CanExecute = nameof(IsRunning))]
    private void TogglePause()
    {
        emulator.PlayMode = emulator.PlayMode == Mupen64PlusTypes.PlayModes.Running
            ? Mupen64PlusTypes.PlayModes.Paused
            : Mupen64PlusTypes.PlayModes.Running;
    }

    private void TryVerifyDependencies(out bool hasAllDependencies)
    {
        // TODO: clean this up

        List<string> missingPlugins = new();

        var coreLibraryExists = File.Exists(generalDependencyContainer.SettingsService.Get<string>("CoreLibraryPath"));

        if (!Path.GetExtension(generalDependencyContainer.SettingsService.Get<string>("CoreLibraryPath"))
                .Equals(".dll", StringComparison.InvariantCultureIgnoreCase)) coreLibraryExists = false;

        if (!coreLibraryExists)
            generalDependencyContainer.DialogService.ShowError(
                generalDependencyContainer.LocalizationService.GetString("CoreLibraryNotFound"));

        var videoPluginExists = File.Exists(generalDependencyContainer.SettingsService.Get<string>("VideoPluginPath"));
        var audioPluginExists = File.Exists(generalDependencyContainer.SettingsService.Get<string>("AudioPluginPath"));
        var inputPluginExists = File.Exists(generalDependencyContainer.SettingsService.Get<string>("InputPluginPath"));
        var rspPluginExists = File.Exists(generalDependencyContainer.SettingsService.Get<string>("RspPluginPath"));
        if (!videoPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationService.GetString("Video"));

        if (!audioPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationService.GetString("Audio"));

        if (!inputPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationService.GetString("Input"));

        if (!rspPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationService.GetString("Rsp"));

        if (!videoPluginExists || !audioPluginExists || !inputPluginExists || (!rspPluginExists && coreLibraryExists))
            generalDependencyContainer.DialogService.ShowError(string.Format(
                generalDependencyContainer.LocalizationService.GetString("PluginNotFoundSeries"),
                string.Join(", ", missingPlugins)));

        hasAllDependencies = coreLibraryExists && videoPluginExists && audioPluginExists && inputPluginExists &&
                             rspPluginExists;
    }
}