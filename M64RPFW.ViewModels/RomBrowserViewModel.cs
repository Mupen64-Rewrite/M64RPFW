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
        List<string> filePaths = new();
        if (SettingsViewModel.Instance.IsRomBrowserRecursive)
        {
            foreach (string directory in SettingsViewModel.Instance.RomBrowserPaths)
            {
                filePaths.AddRange(DirectoryHelper.GetFilesRecursively(directory));
            }
        }
        else
        {
            foreach (string directory in SettingsViewModel.Instance.RomBrowserPaths)
            {
                filePaths.AddRange(Directory.GetFiles(directory));
            }
        }
        filePaths = filePaths.Where(path => _romExtensions.Any(extension => Path.GetExtension(path).Equals(extension, StringComparison.OrdinalIgnoreCase))).ToList();

        return filePaths;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        RomBrowserItemViewModels.Clear();

        foreach (string path in CollectPaths())
        {
            RomBrowserItemViewModels.Add(new RomBrowserItemViewModel(await FileHelpers.ReadSectionAsync(path, 0, 64), path));
        }
    }
}