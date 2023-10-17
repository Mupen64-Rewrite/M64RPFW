using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.ViewModels;

public partial class RomBrowserItemViewModel : ObservableObject
{
    private byte[] _headerData;
    private Mupen64PlusTypes.RomSettings _settings;

    public RomBrowserItemViewModel(byte[] headerData, string path, Mupen64PlusTypes.RomSettings settings)
    {
        _headerData = headerData;
        Path = path;
        _settings = settings;
    }

    public string Path { get; private set; }

    private string? _fileName;
    public string FileName => _fileName ??= System.IO.Path.GetFileName(Path);

    private byte? _countryCode;
    public byte CountryCode => _countryCode ??= _headerData[0x3E];

    private string? _goodName;
    public unsafe string GoodName
    {
        get
        {
            if (_goodName != null)
                return _goodName;
            fixed (byte* pGoodName = _settings.goodname)
            {
                _goodName = CHelpers.DecodeString(pGoodName);
            }
            return _goodName;
        }
    }
}