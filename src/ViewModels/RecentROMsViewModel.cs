using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.src.Interfaces;
using System.Collections.ObjectModel;
using System.IO;

namespace M64RPFW.UI.ViewModels
{
    public partial class RecentROMsViewModel : ObservableObject, IRecentRomsProvider
    {
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
            Properties.Settings.Default.RecentROMPaths.Clear();
            foreach (ROMViewModel item in RecentROMs)
                Properties.Settings.Default.RecentROMPaths.Add(item.Path);
            Properties.Settings.Default.Save();
        }

        public ObservableCollection<ROMViewModel> GetRecentRoms()
        {
            return RecentROMs;
        }

        public RecentROMsViewModel()
        {
            if (Properties.Settings.Default.RecentROMPaths == null)
            {
                Properties.Settings.Default.RecentROMPaths = new();
            }

            foreach (string? recentROMPath in Properties.Settings.Default.RecentROMPaths)
            {
                if (File.Exists(recentROMPath))
                    RecentROMs.Add(new(recentROMPath));
            }
        }
    }

}
