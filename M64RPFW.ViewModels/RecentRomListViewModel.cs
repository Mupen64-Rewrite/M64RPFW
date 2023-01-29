using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.Services.Extensions;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public sealed partial class RecentRomsViewModel : ObservableObject, IRecipient<RomLoadedMessage>, IDisposable
{
    private readonly SettingsViewModel _settingsViewModel;

    public ObservableCollection<RomViewModel> RecentRomViewModels { get; } = new();

    internal RecentRomsViewModel(SettingsViewModel settingsViewModel)
    {
        _settingsViewModel = settingsViewModel;

        RecentRomViewModels.CollectionChanged += OnRecentRomViewModelsCollectionChanged;

        foreach (var recentRomPath in _settingsViewModel.RecentRomPaths)
            try
            {
                RomViewModel rom = new(File.ReadAllBytes(recentRomPath), recentRomPath);
                AppendRomViewModel(rom, false);
            }
            catch
            {
                Debug.Print($"Skipping adding rom {recentRomPath}");
                ; // just... dont add it
            }

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Dispose()
    {
        RecentRomViewModels.CollectionChanged -= OnRecentRomViewModelsCollectionChanged;
    }

    private void AppendRomViewModel(RomViewModel rom, bool addAtHead = true)
    {
        // we don't want invalid roms in the recent rom list
        if (!rom.IsValid) return;

        // reference or path equality = duplicate
        var alreadyExists = RecentRomViewModels.Contains(rom) || RecentRomViewModels.Any(x => x.Path == rom.Path);

        if (alreadyExists)
        {
            // duplicate found
            // should we:
            // A - remove it from the list and later add it back to the head
            // B - cease adding it and don't move anything
            return;
        }

        if (addAtHead)
        {
            RecentRomViewModels.Insert(0, rom);
        }
        else
        {
            RecentRomViewModels.Add(rom);
        }
    }

    [RelayCommand]
    private void RemoveRecentRom(RomViewModel rom)
    {
        _ = RecentRomViewModels.Remove(rom);
    }

    private void OnRecentRomViewModelsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        _settingsViewModel.RecentRomPaths = RecentRomViewModels.Select(x => x.Path).ToList();
        Debug.Print($"Synchronized recent rom paths with {RecentRomViewModels.Count} recent rom vms");
    }


    void IRecipient<RomLoadedMessage>.Receive(RomLoadedMessage message)
    {
        AppendRomViewModel(message.Value);
    }


}