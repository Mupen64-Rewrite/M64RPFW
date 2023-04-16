using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;
using M64RPFW.ViewModels.Helpers;

namespace M64RPFW.ViewModels;

public partial class AdvancedSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ConfigKeyViewModel> _currentSection = new();
    [ObservableProperty] private string _sectionName = "";

    private Dictionary<string, ObservableCollection<ConfigKeyViewModel>> _sectionCache = new();

    partial void OnSectionNameChanged(string value)
    {
        if (!_sectionCache.TryGetValue(value, out var sect))
        {
            sect = MupenConfigHelpers.GenerateConfigSection(Mupen64Plus.ConfigOpenSection(value));
            _sectionCache[value] = sect;
        }

        CurrentSection = sect;
    }
    
    public void OnClosed()
    {
        Mupen64Plus.ConfigSaveFile();
    }

    public IEnumerable<string> ConfigSectionNames => Mupen64Plus.ListConfigSections();
}