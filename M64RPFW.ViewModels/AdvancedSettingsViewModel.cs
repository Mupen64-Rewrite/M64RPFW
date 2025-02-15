using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;
using M64RPFW.ViewModels.Helpers;

namespace M64RPFW.ViewModels;

public partial class AdvancedSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ConfigKeyViewModel> _currentSection = new();
    [ObservableProperty] private string _sectionName = "";

    private readonly List<string> _sectionNames = new();
    private readonly Dictionary<string, ObservableCollection<ConfigKeyViewModel>> _sectionCache = new();

    public AdvancedSettingsViewModel()
    {
        // Load sections
        Mupen64Plus.ConfigForEachSection(_sectionNames.Add);

        if (_sectionNames.Count > 0) SectionName = _sectionNames[0];
    }

    partial void OnSectionNameChanged(string value)
    {
        if (!_sectionCache.TryGetValue(value, out var sect))
        {
            sect = MupenConfigHelpers.GenerateConfigSection(Mupen64Plus.ConfigOpenSection(value));
            _sectionCache[value] = sect;
        }

        CurrentSection = sect;
    }

    public void Save()
    {
        Mupen64Plus.ConfigSaveFile();
    }

    public IEnumerable<string> ConfigSectionNames => _sectionNames;

    public void Revert()
    {
        Mupen64Plus.ConfigForEachSection(sect => Mupen64Plus.ConfigRevertChanges(sect));
    }
}