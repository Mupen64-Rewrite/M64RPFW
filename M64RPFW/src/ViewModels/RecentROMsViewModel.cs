using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.src.Containers;
using M64RPFW.src.Interfaces;
using System.Collections.ObjectModel;
using System.IO;

namespace M64RPFW.UI.ViewModels
{
    public partial class RecentROMsViewModel : ObservableObject, IRecentRomsProvider
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;

        [ObservableProperty]
        private ObservableCollection<ROMViewModel> recentROMs = new();

        [RelayCommand]
        private void RemoveRecentROM(ROMViewModel rom)
        {
            RecentROMs.Remove(rom);
            RegenerateRecentROMPathsSetting();
        }

        public void AddRecentROM(ROMViewModel rom)
        {
            // check for duplicates

            if (recentROMs.Contains(rom))
                return;

            foreach (ROMViewModel item in RecentROMs)
                if (item.Path.Equals(rom.Path)) return;

            RecentROMs.Add(rom);

            RegenerateRecentROMPathsSetting();
        }

        private void RegenerateRecentROMPathsSetting()
        {
            // recreate recent rom list in settings

            generalDependencyContainer.SettingsManager.GetSettings().RecentROMPaths.Clear();
            foreach (ROMViewModel item in RecentROMs)
                generalDependencyContainer.SettingsManager.GetSettings().RecentROMPaths.Add(item.Path);
            generalDependencyContainer.SettingsManager.GetSettings().Save();
        }

        public ObservableCollection<ROMViewModel> GetRecentRoms()
        {
            return RecentROMs;
        }

        internal RecentROMsViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;

            if (generalDependencyContainer.SettingsManager.GetSettings().RecentROMPaths == null)
            {
                generalDependencyContainer.SettingsManager.GetSettings().RecentROMPaths = new();
            }

            foreach (string? recentROMPath in generalDependencyContainer.SettingsManager.GetSettings().RecentROMPaths)
            {
                if (File.Exists(recentROMPath))
                    RecentROMs.Add(new(recentROMPath));
            }
        }
    }

}
