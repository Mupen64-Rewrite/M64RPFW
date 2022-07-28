﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFWAvalonia.Properties;
using M64RPFWAvalonia.ViewModels.Interfaces;
using System.Collections.ObjectModel;
using System.IO;

namespace M64RPFWAvalonia.UI.ViewModels
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
            foreach (ROMViewModel item in RecentROMs)
                if (item.Path.Equals(rom.Path)) return;

            RecentROMs.Add(rom);

            RegenerateRecentROMPathsSetting();
        }

        private void RegenerateRecentROMPathsSetting()
        {
            // recreate recent rom list in settings
            Settings.Default.RecentROMPaths.Clear();
            foreach (ROMViewModel item in RecentROMs)
                Settings.Default.RecentROMPaths.Add(item.Path);
            Settings.Default.Save();
        }

        public ObservableCollection<ROMViewModel> GetRecentRoms()
        {
            return RecentROMs;
        }

        public RecentROMsViewModel()
        {
            if (Settings.Default.RecentROMPaths == null)
            {
                Settings.Default.RecentROMPaths = new();
            }

            foreach (string? recentROMPath in Settings.Default.RecentROMPaths)
            {
                if (File.Exists(recentROMPath))
                    RecentROMs.Add(new(recentROMPath));
            }
        }
    }

}
