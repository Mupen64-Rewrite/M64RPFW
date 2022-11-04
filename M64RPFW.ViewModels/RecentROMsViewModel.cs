using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace M64RPFW.ViewModels
{
    public partial class RecentRomsViewModel : ObservableObject, IRecentRomsProvider
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;

        [ObservableProperty]
        private ObservableCollection<RomViewModel> recentRoms = new();

        [RelayCommand]
        private void RemoveRecentRom(RomViewModel Rom)
        {
            _ = recentRoms.Remove(Rom);
            RegenerateRecentRomPathsSetting();
        }

        private void RegenerateRecentRomPathsSetting()
        {
            // recreate recent Rom list in settings

            generalDependencyContainer.SettingsService.Set("RecentRomPaths", Array.Empty<string>());

            foreach (RomViewModel item in recentRoms)
            {
                List<string> paths = generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths").ToList();
                paths.Add(item.Path);
                generalDependencyContainer.SettingsService.Set("RecentRomPaths", paths.ToArray());
            }

            generalDependencyContainer.SettingsService.Save();
        }

        public ObservableCollection<RomViewModel> GetRecentRoms()
        {
            return recentRoms;
        }

        ObservableCollection<RomViewModel> IRecentRomsProvider.Get()
        {
            return recentRoms;
        }

        public void Add(RomViewModel Rom)
        {
            // sanity checks
            if (!Rom.IsValid)
            {
                return;
            }

            // don't add the Rom if any duplicates are found
            if (recentRoms.Contains(Rom))
            {
                return;
            }

            foreach (var _ in recentRoms.Where(item => item.Path == Rom.Path).Select(item => new { }))
            {
                return;
            }

            recentRoms.Add(Rom);

            RegenerateRecentRomPathsSetting();
        }

        internal RecentRomsViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;

            if (generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths") == null)
            {
                generalDependencyContainer.SettingsService.Set("RecentRomPaths", Array.Empty<string>());
            }

            foreach (string? recentRomPath in generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths").ToList())
            {
                try
                {
                    RomViewModel Rom = new(File.ReadAllBytes(recentRomPath), recentRomPath);
                    Add(Rom);
                }
                catch
                {
                    ; // just... dont add it
                }
            }
        }
    }

}
