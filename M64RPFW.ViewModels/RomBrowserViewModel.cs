using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        
        List<string> filePaths = new();
        foreach (string directory in SettingsViewModel.Instance.RomBrowserPaths)
        {
            filePaths.AddRange(Directory.GetFiles(directory, "*.*", searchOption)
                .Where(path => _romExtensions.Any(extension => Path.GetExtension(path).Equals(extension, StringComparison.OrdinalIgnoreCase))));
        }

        return filePaths;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        RomBrowserItemViewModels.Clear();

        var paths = await Task.Run(CollectPaths);

        foreach (string path in CollectPaths())
        {
            RomBrowserItemViewModels.Add(new RomBrowserItemViewModel(await FileHelpers.ReadSectionAsync(path, 0, 64), path));
        }
    }
}