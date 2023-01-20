using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Services.Extensions;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public partial class RecentRomsViewModel : ObservableObject, IRecipient<RomLoadedMessage>
{
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    public ObservableCollection<RomViewModel> RecentRomViewModels
    {
        get;
        private set;
    } = new();

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
                Add(rom);
            }
            catch
            {
                Debug.WriteLine($"Skipping adding rom {recentRomPath}");
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
        if (RecentRomViewModels.Contains(rom)) return;

        // path duplication check
        if (RecentRomViewModels.Any(x => x.Path == rom.Path))
        {
            return;
        }

        // no duplicates found, add it
        RecentRomViewModels.Insert(0, rom);

        SyncSettingWithInternalArray(doSave);
    }

    [RelayCommand]
    private void RemoveRecentRom(RomViewModel rom)
    {
        _ = RecentRomViewModels.Remove(rom);
        SyncSettingWithInternalArray();
    }

    private void SyncSettingWithInternalArray(bool doSave = true)
    {
        var paths = RecentRomViewModels.Select(x => x.Path);
        _generalDependencyContainer.SettingsService.Set("RecentRomPaths", paths.ToArray());
        if (doSave)
        {
            _generalDependencyContainer.SettingsService.Save();
        }
    }

    public void Receive(RomLoadedMessage message)
    {
        Add(message.Value);
    }
}