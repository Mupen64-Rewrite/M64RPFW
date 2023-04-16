using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;

namespace M64RPFW.ViewModels;

public class ConfigKeyViewModel : ObservableObject
{
    public ConfigKeyViewModel(IntPtr section, string name)
    {
        _section = section;
        Name = name;
    }

    private readonly IntPtr _section;
    
    public string Name { get; }

    public object Value
    {
        get => Mupen64Plus.ConfigGetObject(_section, Name)!;
        set
        {
            Mupen64Plus.ConfigSetObject(_section, Name, value);
            OnPropertyChanged();
        }
    }
}

