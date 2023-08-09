using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.ViewModels.Helpers;

namespace M64RPFW.ViewModels;

public partial class RomBrowserViewModel : ObservableObject
{
    private readonly string[] _romExtensions =
    {
        ".z64",
        ".n64",
        ".eur",
        ".v64"
    };

    public ObservableCollection<RomBrowserItemViewModel> RomBrowserItemViewModels { get; } = new();

    private IEnumerable<string> CollectPaths()
    {
        var searchOption = SettingsViewModel.Instance.IsRomBrowserRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (string directory in SettingsViewModel.Instance.RomBrowserPaths)
        {
            foreach (string file in Directory.EnumerateFiles(directory, "*.*", searchOption))
            {
                if (_romExtensions.Any(extension => Path.GetExtension(file).Equals(extension, StringComparison.OrdinalIgnoreCase)))
                    yield return file;
            }
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (Mupen64Plus.CoreStateQuery(Mupen64PlusTypes.CoreParam.EmuState) != (int) Mupen64PlusTypes.EmuState.Stopped)
            return;
        RomBrowserItemViewModels.Clear();
        
        // This might not be optimal with bad disk speeds.
        foreach (string path in CollectPaths())
        {
            RomBrowserItemViewModels.Add(new RomBrowserItemViewModel(await FileHelpers.ReadSectionAsync(path, 0, 64), path));
        }
    }
}