using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.UI.ViewModels.Extensions.Localization;
using M64RPFW.UI.Views;
using M64RPFW.UI.Views.Plugins;
using M64RPFW.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace M64RPFW.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public EmulatorViewModel EmulatorViewModel { get; private set; }
        public SavestatesViewModel SavestatesViewModel { get; private set; }
        public RecentROMsViewModel RecentROMsViewModel { get; private set; }

        [ObservableProperty]
        private ObservableCollection<ROMViewModel> recentROMs = new();

        public MainViewModel()
        {
            LocalizationManager.SetCulture(Properties.Settings.Default.Culture);
            RecentROMsViewModel = new();
            EmulatorViewModel = new(RecentROMsViewModel);
            SavestatesViewModel = new();
        }

        [RelayCommand]
        private void ExitApp() => Application.Current.MainWindow.Close();

        [RelayCommand]
        private void ShowSettingsWindow() => new SettingsWindow() { DataContext = new SettingsViewModel() }.ShowDialog();

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
