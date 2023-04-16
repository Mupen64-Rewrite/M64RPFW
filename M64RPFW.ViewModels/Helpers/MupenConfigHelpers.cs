using M64RPFW.Models.Emulation;

namespace M64RPFW.ViewModels.Helpers;

public static class MupenConfigHelpers
{
    
    public static void ConfigSectionToDict(IntPtr section, IDictionary<string, object> dict)
    {
        if (section == IntPtr.Zero)
            throw new ArgumentException("Cannot read null config section", nameof(section));
        ArgumentNullException.ThrowIfNull(dict);
        
        Mupen64Plus.ConfigCallOverParameters(section, (name, _) =>
        {
            dict[name] = Mupen64Plus.ConfigGetObject(section, name)!;
        });
    }

    public static void DictToConfigSection(IDictionary<string, object> dict, IntPtr section)
    {
        if (section == IntPtr.Zero)
            throw new ArgumentException("Cannot read null config section", nameof(section));
        ArgumentNullException.ThrowIfNull(dict);
        
        foreach (var (key, value) in dict)
        {
            Mupen64Plus.ConfigSetObject(section, key, value);
        }
    }
}