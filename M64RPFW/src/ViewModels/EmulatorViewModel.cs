﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Emulation.API;
using M64RPFW.Models.Helpers;
using M64RPFW.src.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace M64RPFW.UI.ViewModels
{
    internal partial class EmulatorViewModel : ObservableObject
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;
        private readonly Emulator emulator;

        internal EmulatorViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;
            emulator = new();
            emulator.IsRunningChanged += IsRunningChanged;
            emulator.PlayModeChanged += PlayModeChanged;
        }

        public bool IsRunning => emulator.IsRunning;
        private void IsRunningChanged()
        {
            generalDependencyContainer.UIThreadDispatcherProvider.Execute(() =>
            {
                OnPropertyChanged(nameof(IsRunning));
                ICommandHelper.NotifyCanExecuteChanged(CloseROMCommand, ResetROMCommand, FrameAdvanceCommand, TogglePauseCommand);
            });
        }

        public bool IsResumed
        {
            get => emulator.PlayMode == Mupen64PlusTypes.PlayModes.Running;
            set => emulator.PlayMode = (value ? Mupen64PlusTypes.PlayModes.Running : Mupen64PlusTypes.PlayModes.Paused);
        }
        private void PlayModeChanged()
        {
            generalDependencyContainer.UIThreadDispatcherProvider.Execute(() =>
            {
                OnPropertyChanged(nameof(IsResumed));
            });
        }


        [RelayCommand]
        private void LoadROM()
        {
            (string ReturnedPath, bool Cancelled) = generalDependencyContainer.FileDialogProvider.OpenFileDialogPrompt(generalDependencyContainer.RomFileExtensionsConfigurationProvider.ROMFileExtensionsConfiguration.FileExtensions);
            if (Cancelled) return;
            LoadROMFromPath(ReturnedPath);
        }

        [RelayCommand]
        private void LoadROMFromPath(string path)
        {
            if (!AreAllDependenciesMet()) return;

            ROMViewModel rom = new(path);

            if (!new ROMViewModel(path).IsValid)
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(generalDependencyContainer.LocalizationProvider.GetString("ROMInexistent"));
                return;
            }

            generalDependencyContainer.RecentRomsProvider.AddRecentROM(rom);  // add recent rom here, but not in LoadROMFromPath because the latter is called by recent rom module itself

            if (IsRunning)
            {
                emulator.Stop();
            }

            var config = new Mupen64PlusConfig(generalDependencyContainer.SettingsManager.GetSettings().CoreType, generalDependencyContainer.SettingsManager.GetSettings().ScreenWidth, generalDependencyContainer.SettingsManager.GetSettings().ScreenHeight);
            var launchParameters = new Mupen64PlusLaunchParameters(File.ReadAllBytes(path),
                                                                             config,
                                                                             generalDependencyContainer.SettingsManager.GetSettings().DefaultSlot,
                                                                             generalDependencyContainer.SettingsManager.GetSettings().CoreLibraryPath,
                                                                             generalDependencyContainer.SettingsManager.GetSettings().VideoPluginPath,
                                                                             generalDependencyContainer.SettingsManager.GetSettings().AudioPluginPath,
                                                                             generalDependencyContainer.SettingsManager.GetSettings().InputPluginPath,
                                                                             generalDependencyContainer.SettingsManager.GetSettings().RSPPluginPath);
            emulator.Start(launchParameters);

            emulator.API.OnFrameBufferCreated += delegate
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
            if (Properties.Settings.Default.PauseOnFrameAdvance)
                emulator.PlayMode = Mupen64PlusTypes.PlayModes.Paused;
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

            bool coreLibraryExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().CoreLibraryPath);

            if (!Path.GetExtension(generalDependencyContainer.SettingsManager.GetSettings().CoreLibraryPath).Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
            {
                coreLibraryExists = false;
            }

            if (!coreLibraryExists)
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(generalDependencyContainer.LocalizationProvider.GetString("CoreLibraryNotFound"));
            }

            bool videoPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().VideoPluginPath);
            bool audioPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().AudioPluginPath);
            bool inputPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().InputPluginPath);
            bool rspPluginExists = File.Exists(generalDependencyContainer.SettingsManager.GetSettings().RSPPluginPath);
            if (!videoPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("Video"));
            if (!audioPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("Audio"));
            if (!inputPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("Input"));
            if (!rspPluginExists) missingPlugins.Add(generalDependencyContainer.LocalizationProvider.GetString("RSP"));

            if (!videoPluginExists || !audioPluginExists || !inputPluginExists || (!rspPluginExists && coreLibraryExists))
            {
                generalDependencyContainer.DialogProvider.ShowErrorDialog(string.Format(Properties.Resources.PluginNotFoundSeries, string.Join(", ", missingPlugins)));
            }

            return coreLibraryExists && videoPluginExists && audioPluginExists && inputPluginExists && rspPluginExists;
        }
    }
}
