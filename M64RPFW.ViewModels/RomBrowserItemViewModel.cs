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
    private byte[] _rawData;

    public RomBrowserItemViewModel(byte[] rawData, string path)
    {
        _rawData = rawData;
        Path = path;
    }

    public string Path { get; private set; }

    private string? _fileName;
    public string FileName => _fileName ??= System.IO.Path.GetFileName(Path);

    private uint? _crc1;
    public uint CRC1 => _crc1 ??= BinaryPrimitives.ReadUInt32BigEndian(new ReadOnlySpan<byte>(_rawData, 0x10, 0x04));

    private uint? _crc2;
    public uint CRC2 => _crc2 ??= BinaryPrimitives.ReadUInt32BigEndian(new ReadOnlySpan<byte>(_rawData, 0x14, 0x04));

    private Mupen64PlusTypes.RomSettings _settings;
    private bool _settingsSet;
    public ref Mupen64PlusTypes.RomSettings Settings
    {
        get
        {
            if (_settingsSet)
                return ref _settings;
            Mupen64Plus.GetRomSettings(out _settings, CRC1, CRC2);
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