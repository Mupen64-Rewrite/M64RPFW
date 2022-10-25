using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace M64RPFW.ViewModels
{
    public partial class RecentROMsViewModel : ObservableObject, IRecentRomsProvider
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;

        [ObservableProperty]
        private ObservableCollection<ROMViewModel> recentROMs = new();

        [RelayCommand]
        private void RemoveRecentROM(ROMViewModel rom)
        {
            _ = RecentROMs.Remove(rom);
            RegenerateRecentROMPathsSetting();
        }

        private void RegenerateRecentROMPathsSetting()
        {
            // recreate recent rom list in settings

            generalDependencyContainer.SettingsManager.SetSetting("RecentROMPaths", Array.Empty<string>());

            foreach (ROMViewModel item in RecentROMs)
            {
                List<string> paths = generalDependencyContainer.SettingsManager.GetSetting<string[]>("RecentROMPaths").ToList();
                paths.Add(item.Path);
                generalDependencyContainer.SettingsManager.SetSetting("RecentROMPaths", paths.ToArray());
            }

            generalDependencyContainer.SettingsManager.Save();
        }

        public ObservableCollection<ROMViewModel> GetRecentRoms()
        {
            return RecentROMs;
        }

        ObservableCollection<ROMViewModel> IRecentRomsProvider.GetRecentRoms()
        {
            return RecentROMs;
        }

        public void AddRecentROM(ROMViewModel rom)
        {
            // sanity checks
            if (!rom.IsValid)
            {
                return;
            }

            // don't add the rom if any duplicates are found
            if (recentROMs.Contains(rom))
            {
                return;
            }

            foreach (var _ in RecentROMs.Where(item => item.Path == rom.Path).Select(item => new { }))
            {
                return;
            }

            RecentROMs.Add(rom);

            RegenerateRecentROMPathsSetting();
        }

        internal RecentROMsViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;

            if (generalDependencyContainer.SettingsManager.GetSetting<string[]>("RecentROMPaths") == null)
            {
                generalDependencyContainer.SettingsManager.SetSetting("RecentROMPaths", Array.Empty<string>());
            }

            foreach (string? recentROMPath in generalDependencyContainer.SettingsManager.GetSetting<string[]>("RecentROMPaths").ToList())
            {
                try
                {
                    ROMViewModel rom = new(File.ReadAllBytes(recentROMPath), recentROMPath);
                    AddRecentROM(rom);
                }
                catch
                {
                    ; // just... dont add it
                }
            }
        }
    }

}
