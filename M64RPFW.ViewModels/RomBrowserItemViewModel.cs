using CommunityToolkit.Mvvm.ComponentModel;

namespace M64RPFW.ViewModels;

public partial class RomBrowserItemViewModel : ObservableObject
{
    private byte[] _rawData;

    public RomBrowserItemViewModel(byte[] rawData, string path)
    {
        _rawData = rawData;
        Path = path;
    }

    public string Path { get; set; }
}