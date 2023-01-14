using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Interfaces;

namespace M64RPFW.ViewModels;

public partial class RecentRomsViewModel : ObservableObject, IRecentRomsProvider
{
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    [ObservableProperty] private ObservableCollection<RomViewModel> _recentRoms = new();

    internal RecentRomsViewModel(GeneralDependencyContainer generalDependencyContainer)
    {
        this._generalDependencyContainer = generalDependencyContainer;

        if (generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths") == null)
            generalDependencyContainer.SettingsService.Set("RecentRomPaths", Array.Empty<string>());

        foreach (var recentRomPath in generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths")
                     .ToList())
            try
            {
                RomViewModel rom = new(File.ReadAllBytes(recentRomPath), recentRomPath);
                Add(rom);
            }
            catch
            {
                ; // just... dont add it
            }
    }

    ObservableCollection<RomViewModel> IRecentRomsProvider.Get()
    {
        return _recentRoms;
    }

    public void Add(RomViewModel rom)
    {
        // sanity checks
        if (!rom.IsValid) return;

        // don't add the Rom if any duplicates are found
        if (_recentRoms.Contains(rom)) return;

        foreach (var _ in _recentRoms.Where(item => item.Path == rom.Path).Select(item => new { })) return;

        _recentRoms.Add(rom);

        RegenerateRecentRomPathsSetting();
    }

    [RelayCommand]
    private void RemoveRecentRom(RomViewModel rom)
    {
        _ = _recentRoms.Remove(rom);
        RegenerateRecentRomPathsSetting();
    }

    private void RegenerateRecentRomPathsSetting()
    {
        // recreate recent Rom list in settings

        _generalDependencyContainer.SettingsService.Set("RecentRomPaths", Array.Empty<string>());

        foreach (var item in _recentRoms)
        {
            var paths = _generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths").ToList();
            paths.Add(item.Path);
            _generalDependencyContainer.SettingsService.Set("RecentRomPaths", paths.ToArray());
        }

        _generalDependencyContainer.SettingsService.Save();
    }

    public ObservableCollection<RomViewModel> GetRecentRoms()
    {
        return _recentRoms;
    }
}