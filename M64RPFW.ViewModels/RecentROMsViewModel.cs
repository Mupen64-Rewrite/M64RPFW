using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Services.Extensions;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Interfaces;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public partial class RecentRomsViewModel : ObservableObject, IRecipient<RomLoadedMessage>
{
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    [ObservableProperty] private ObservableCollection<RomViewModel> _recentRoms = new();

    internal RecentRomsViewModel(GeneralDependencyContainer generalDependencyContainer)
    {
        this._generalDependencyContainer = generalDependencyContainer;

        if (!generalDependencyContainer.SettingsService.TryGet<string[]>("RecentRomPaths", out var recentRomPaths))
        {
            generalDependencyContainer.SettingsService.Set("RecentRomPaths", Array.Empty<string>());
        }
        
        foreach (var recentRomPath in generalDependencyContainer.SettingsService.Get<string[]>("RecentRomPaths")
                     .ToList())
            try
            {
                RomViewModel rom = new(File.ReadAllBytes(recentRomPath), recentRomPath);
                Add(rom, false);
            }
            catch
            {
                ; // just... dont add it
            }
        
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Add(RomViewModel rom, bool doSave = true)
    {
        // sanity checks
        if (!rom.IsValid) return;

        // check for duplicates
        
        // reference duplication check 
        if (_recentRoms.Contains(rom)) return;

        // path duplication check
        if (_recentRoms.Any(x => x.Path == rom.Path))
        {
            return;
        }

        // no duplicates found, add it
        _recentRoms.Insert(0, rom);

        SyncSettingWithInternalArray(doSave);
    }

    [RelayCommand]
    private void RemoveRecentRom(RomViewModel rom)
    {
        _ = _recentRoms.Remove(rom);
        SyncSettingWithInternalArray();
    }

    private void SyncSettingWithInternalArray(bool doSave = true)
    {
        var paths = _recentRoms.Select(x => x.Path);
        _generalDependencyContainer.SettingsService.Set("RecentRomPaths", paths.ToArray());
        if (doSave)
        {
            _generalDependencyContainer.SettingsService.Save();
        }
    }

    public ObservableCollection<RomViewModel> GetRecentRoms()
    {
        return _recentRoms;
    }

    public void Receive(RomLoadedMessage message)
    {
        Add(message.Value);
    }
}