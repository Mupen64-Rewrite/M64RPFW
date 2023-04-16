using System.Collections.ObjectModel;
using M64RPFW.Models.Emulation;

namespace M64RPFW.ViewModels.Helpers;

public static class MupenConfigHelpers
{
    public static ObservableCollection<ConfigKeyViewModel> GenerateConfigSection(IntPtr section)
    {
        ObservableCollection<ConfigKeyViewModel> result = new();
        Mupen64Plus.ConfigCallOverParameters(section, (name, _) =>
        {
            result.Add(new ConfigKeyViewModel(section, name));
        });
        return result;
    }
}