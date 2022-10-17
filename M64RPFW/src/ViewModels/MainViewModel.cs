﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.src.Containers;
using M64RPFW.UI.ViewModels.Extensions.Localization;
using M64RPFW.UI.Views;
using M64RPFW.UI.Views.Plugins;
using M64RPFW.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace M64RPFW.UI.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;

        public EmulatorViewModel EmulatorViewModel { get; private set; }
        public SavestatesViewModel SavestatesViewModel { get; private set; }
        public RecentROMsViewModel RecentROMsViewModel { get; private set; }

        [ObservableProperty]
        private ObservableCollection<ROMViewModel> recentROMs = new();

        public MainViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;

            RecentROMsViewModel = new(generalDependencyContainer);

            generalDependencyContainer.RecentRomsProvider = RecentROMsViewModel;

            EmulatorViewModel = new(generalDependencyContainer);
            SavestatesViewModel = new();

        }

        [RelayCommand]
        private void ExitApp() => generalDependencyContainer.WindowClosingProvider.Close();

        [RelayCommand]
        private void ShowSettingsWindow() => new SettingsWindow() { DataContext = new SettingsViewModel(generalDependencyContainer) }.ShowDialog();

        [RelayCommand]
        private void ShowVideoPluginConfigurationWindow() => new VideoPluginConfigurationWindow() { DataContext = null }.ShowDialog();
        [RelayCommand]
        private void ShowAudioPluginConfigurationWindow() => new AudioPluginConfigurationWindow() { DataContext = null }.ShowDialog();
        [RelayCommand]
        private void ShowInputPluginConfigurationWindow() => new InputPluginConfigurationWindow() { DataContext = null }.ShowDialog();
        [RelayCommand]
        private void ShowRSPPluginConfigurationWindow() => new RSPPluginConfigurationWindow() { DataContext = null }.ShowDialog();
        [RelayCommand]
        private void ShowROMInspectionWindow(ROMViewModel rom) => new ROMInspectionWindow() { DataContext = rom }.Show();


    }
}