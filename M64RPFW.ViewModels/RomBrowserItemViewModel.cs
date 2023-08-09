using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.ViewModels;

public partial class RomBrowserItemViewModel : ObservableObject
{
    private byte[] _headerData;

    public RomBrowserItemViewModel(byte[] headerData, string path)
    {
        _headerData = headerData;
        Path = path;
    }

    public string Path { get; private set; }

    private string? _fileName;
    public string FileName => _fileName ??= System.IO.Path.GetFileName(Path);

    private byte? _countryCode;
    public byte CountryCode => _countryCode ??= _headerData[0x3E];

    private Mupen64PlusTypes.RomSettings _settings;
    private bool _settingsSet;
    public ref Mupen64PlusTypes.RomSettings Settings
    {
        get
        {
            if (_settingsSet)
                return ref _settings;
            try
            {
                Mupen64Plus.OpenRom(Path);
                Mupen64Plus.GetRomSettings(out _settings);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Mupen64Plus.CloseRom();
            }
            _settingsSet = true;
            return ref _settings;
        }
    }

    private string? _goodName;
    public unsafe string GoodName
    {
        get
        {
            if (_goodName != null)
                return _goodName;
            fixed (byte* pGoodName = Settings.goodname)
            {
                _goodName = CHelpers.DecodeString(pGoodName);
            }
            return _goodName;
        }
    }
}