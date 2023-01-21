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
    private readonly SettingsViewModel _settingsViewModel;

    public ObservableCollection<RomViewModel> RecentRomViewModels
    {
        get;
    } = new();

    internal RecentRomsViewModel(SettingsViewModel settingsViewModel)
    {
        _settingsViewModel = settingsViewModel;
        
        foreach (var recentRomPath in _settingsViewModel.RecentRomPaths
                     .ToList())
            try
            {
                RomViewModel rom = new(File.ReadAllBytes(recentRomPath), recentRomPath);
                Add(rom, false);
            }
            catch
            {
                Debug.WriteLine($"Skipping adding rom {recentRomPath}");
                ; // just... dont add it
            }
        
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Add(RomViewModel rom, bool addAtHead = true)
    {
        // sanity checks
        if (!rom.IsValid) return;

        // check for duplicates
        bool alreadyExists = RecentRomViewModels.Contains(rom) || RecentRomViewModels.Any(x => x.Path == rom.Path);

        if (alreadyExists)
        {
            // duplicate found
            // should we:
            // A - remove it from the list and later add it back to the head
            // B - cease adding it and don't move anything
            return;
            //RecentRomViewModels.Remove(rom);
        }

        if (addAtHead)
        {
            RecentRomViewModels.Insert(0, rom);
        }
        else
        {
            RecentRomViewModels.Add(rom);
        }
        
        
        SyncSettingWithInternalArray();
    }

    [RelayCommand]
    private void RemoveRecentRom(RomViewModel rom)
    {
        _ = RecentRomViewModels.Remove(rom);
        SyncSettingWithInternalArray();
    }

    private void SyncSettingWithInternalArray()
    {
        _settingsViewModel.RecentRomPaths = RecentRomViewModels.Select(x => x.Path).ToArray();
    }

    public void Receive(RomLoadedMessage message)
    {
        Add(message.Value);
    }
}