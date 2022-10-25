using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Emulation.API;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace M64RPFW.ViewModels
{
    public partial class EmulatorViewModel : ObservableObject
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;
        private readonly Emulator emulator;
        private readonly MainViewModel mainViewModel;

        internal EmulatorViewModel(GeneralDependencyContainer generalDependencyContainer, MainViewModel mainViewModel)
        {
            this.generalDependencyContainer = generalDependencyContainer;
            this.mainViewModel = mainViewModel;

            mainViewModel.OnWindowExit += delegate
            {
                CloseROMCommand.Execute(null);
            };

            emulator = new();
            emulator.PlayModeChanged += PlayModeChanged;
        }

        public bool IsRunning => emulator.PlayMode != Mupen64PlusTypes.PlayModes.Stopped;
        public bool IsResumed
        {
            get => emulator.PlayMode == Mupen64PlusTypes.PlayModes.Running;
            set => emulator.PlayMode = value ? Mupen64PlusTypes.PlayModes.Running : Mupen64PlusTypes.PlayModes.Paused;
        }
        private void PlayModeChanged()
        {
            generalDependencyContainer.UIThreadDispatcherProvider.Execute(() =>
            {
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(IsResumed));
                ICommandHelper.NotifyCanExecuteChanged(CloseROMCommand, ResetROMCommand, FrameAdvanceCommand, TogglePauseCommand);

            });
        }


        [RelayCommand]
        private void LoadROM()
        {
            (string ReturnedPath, bool Cancelled) = generalDependencyContainer.FileDialogProvider.OpenFileDialogPrompt(generalDependencyContainer.RomFileExtensionsConfigurationProvider.ROMFileExtensionsConfiguration.FileExtensions);
            if (Cancelled)
            {
                return;
            }

            LoadROMFromPath(ReturnedPath);
        }

        [RelayCommand]
        private void LoadROMFromPath(string path)
        {
            if (!AreAllDependenciesMet())
            {
                return;
            }

            void ShowInvalidFileError()
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(generalDependencyContainer.LocalizationProvider.GetString("InvalidFile"));
            }

            // awfull 

            ROMViewModel? rom = null;

            try
            {
                rom = new(File.ReadAllBytes(path), path);
            }
            catch
            {
                ShowInvalidFileError();
                return;
            }

            if (!rom.IsValid)
            {
                ShowInvalidFileError();
                return;
            }

            generalDependencyContainer.RecentRomsProvider.AddRecentROM(rom);

            if (IsRunning)
            {
                emulator.Stop();
            }

            Mupen64PlusConfig config = new(generalDependencyContainer.SettingsManager.GetSetting<int>("CoreType"), generalDependencyContainer.SettingsManager.GetSetting<int>("ScreenWidth"), generalDependencyContainer.SettingsManager.GetSetting<int>("ScreenHeight"));
            Mupen64PlusLaunchParameters launchParameters = new(File.ReadAllBytes(path),
                                                                             config,
                                                                             generalDependencyContainer.SettingsManager.GetSetting<int>("DefaultSlot"),
                                                                             generalDependencyContainer.SettingsManager.GetSetting<string>("CoreLibraryPath"),
                                                                             generalDependencyContainer.SettingsManager.GetSetting<string>("VideoPluginPath"),
                                                                             generalDependencyContainer.SettingsManager.GetSetting<string>("AudioPluginPath"),
                                                                             generalDependencyContainer.SettingsManager.GetSetting<string>("InputPluginPath"),
                                                                             generalDependencyContainer.SettingsManager.GetSetting<string>("RSPPluginPath"));
            emulator.Start(launchParameters);

            emulator.API.OnFrameBufferCreate += delegate
            {
                generalDependencyContainer.DrawingSurfaceProvider.Create(emulator.API.BufferWidth, emulator.API.BufferHeight);
            };
            emulator.API.OnFrameBufferUpdate += delegate
            {
                generalDependencyContainer.DrawingSurfaceProvider.Draw(emulator.API.FrameBuffer, emulator.API.BufferWidth, emulator.API.BufferHeight);
            };
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void CloseROM()
        {
            emulator.Stop();
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void ResetROM()
        {
            emulator.Reset();
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void FrameAdvance()
        {
            if (generalDependencyContainer.SettingsManager.GetSetting<bool>("PauseOnFrameAdvance"))
            {
                emulator.PlayMode = Mupen64PlusTypes.PlayModes.Paused;
            }

            emulator.API.FrameAdvance();
        }

        [RelayCommand(CanExecute = nameof(IsRunning))]
        private void TogglePause()
        {
            emulator.PlayMode = emulator.PlayMode == Mupen64PlusTypes.PlayModes.Running ? Mupen64PlusTypes.PlayModes.Paused : Mupen64PlusTypes.PlayModes.Running;
        }

        private bool AreAllDependenciesMet()
        {
            // TODO: clean this up

            List<string> missingPlugins = new();

            bool coreLibraryExists = File.Exists(generalDependencyContainer.SettingsManager.GetSetting<string>("CoreLibraryPath"));

            if (!Path.GetExtension(generalDependencyContainer.SettingsManager.GetSetting<string>("CoreLibraryPath")).Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                coreLibraryExists = false;
            }

            if (!coreLibraryExists)
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(generalDependencyContainer.LocalizationProvider.GetString("CoreLibraryNotFound"));
            }

            bool videoPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSetting<string>("VideoPluginPath"));
            bool audioPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSetting<string>("AudioPluginPath"));
            bool inputPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSetting<string>("InputPluginPath"));
            bool rspPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSetting<string>("RSPPluginPath"));
            if (!videoPluginExists)
            {
                missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("Video"));
            }

            if (!audioPluginExists)
            {
                missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("Audio"));
            }

            if (!inputPluginExists)
            {
                missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("Input"));
            }

            if (!rspPluginExists)
            {
                missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("RSP"));
            }

            if (!videoPluginExists || !audioPluginExists || !inputPluginExists || (!rspPluginExists && coreLibraryExists))
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(string.Format(generalDependencyContainer.LocalizationProvider.GetString("PluginNotFoundSeries"), string.Join(", ", missingPlugins)));
            }

            return coreLibraryExists && videoPluginExists && audioPluginExists && inputPluginExists && rspPluginExists;
        }
    }
}
