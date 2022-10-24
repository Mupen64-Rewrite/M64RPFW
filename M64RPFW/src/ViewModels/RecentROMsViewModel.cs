using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.src.Containers;
using M64RPFW.src.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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
            // don't add the rom if any duplicates are found
            if (recentROMs.Contains(rom))
                return;
            foreach (var _ in RecentROMs.Where(item => item.Path == rom.Path).Select(item => new { }))
            {
                return;
            }

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

            generalDependencyContainer.SettingsManager.GetSettings().RecentROMPaths ??= new();

            foreach (string? recentROMPath in generalDependencyContainer.SettingsManager.GetSettings().RecentROMPaths.Cast<string>().ToList())
            {
                try
                {
                    var rom = new ROMViewModel(File.ReadAllBytes(recentROMPath), recentROMPath);
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
