using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Interfaces;

namespace M64RPFW.ViewModels;

public partial class RecentRomsViewModel : ObservableObject, IRecentRomsProvider
{
    private readonly GeneralDependencyContainer generalDependencyContainer;

    [ObservableProperty] private ObservableCollection<RomViewModel> recentRoms = new();

    internal RecentRomsViewModel(GeneralDependencyContainer generalDependencyContainer)
    {
        this.generalDependencyContainer = generalDependencyContainer;

        if (generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths") == null)
            generalDependencyContainer.SettingsService.Set("RecentRomPaths", Array.Empty<string>());

        foreach (var recentRomPath in generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths")
                     .ToList())
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

    ObservableCollection<RomViewModel> IRecentRomsProvider.Get()
    {
        return recentRoms;
    }

    public void Add(RomViewModel Rom)
    {
        // sanity checks
        if (!Rom.IsValid) return;

        // don't add the Rom if any duplicates are found
        if (recentRoms.Contains(Rom)) return;

        foreach (var _ in recentRoms.Where(item => item.Path == Rom.Path).Select(item => new { })) return;

        recentRoms.Add(Rom);

        RegenerateRecentRomPathsSetting();
    }

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

        foreach (var item in recentRoms)
        {
            var paths = generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths").ToList();
            paths.Add(item.Path);
            generalDependencyContainer.SettingsService.Set("RecentRomPaths", paths.ToArray());
        }

        generalDependencyContainer.SettingsService.Save();
    }

    public ObservableCollection<RomViewModel> GetRecentRoms()
    {
        return recentRoms;
    }
}