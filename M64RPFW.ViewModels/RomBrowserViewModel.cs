using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
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

        using var memBuffer = new MemoryStream();
        foreach (string path in CollectPaths())
        {
            using (var inStream = File.OpenRead(path))
            {
                memBuffer.SetLength(0);
                memBuffer.Seek(0, SeekOrigin.Begin);
                await inStream.CopyToAsync(memBuffer);
            }
            var settings = await Task.Run(() =>
            {
                RomHelper.AdaptiveByteSwap(memBuffer.GetBuffer());
                // We need to open the ROM, as M64+ does not allow us to independently calculate MD5 hashes
                // and look them up in the ROM database. This does mean that the hashes have to be recomputed
                // every single time.
                Mupen64Plus.OpenRomBinary(new ReadOnlySpan<byte>(memBuffer.GetBuffer(), 0, (int) memBuffer.Length));
                Mupen64Plus.GetRomSettings(out var settings);
                Mupen64Plus.CloseRom();
                return settings;
            });
            RomBrowserItemViewModels.Add(new RomBrowserItemViewModel(memBuffer.GetBuffer()[..64], path, settings));
        }
    }
}