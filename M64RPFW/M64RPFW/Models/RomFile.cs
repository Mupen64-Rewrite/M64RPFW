using System;
using System.Globalization;
using System.IO;
using System.Text;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Models.Helpers;

namespace M64RPFW.Models;

public class RomFile
{
    public enum RegionEnum
    {
        None = 0,
        Japan,
        USA,
        Europe,
        Germany,
        Australia,
        Italy,
        France,
        Spain
    }
    
    public RomFile(string path)
    {
        string fullPath = Path.GetFullPath(path);
        Console.WriteLine($"Loading ROM from {fullPath}");

        _path = fullPath;
        _data = File.ReadAllBytes(fullPath);

        if (_data.Length < 0x1000)
            throw new ArgumentException("Data is too short to be a ROM!");
        
        RomHelper.AdaptiveByteSwap(ref _data);
    }
    
    public RegionEnum Region => _data[0x3E] switch
    {
        0x44 => RegionEnum.Germany,
        0x45 => RegionEnum.USA,
        0x4A => RegionEnum.Japan,
        0x20 or 0x21 or 0x38 or 
            0x78 or 0x50 or 0x58 => RegionEnum.Europe,
        0x55 => RegionEnum.Australia,
        0x49 => RegionEnum.Italy,
        0x46 => RegionEnum.France,
        0x53 => RegionEnum.Spain,
        _ => RegionEnum.None
    };

    public void LoadThisRom()
    {
        Mupen64Plus.OpenRomBinary(_data);
    }

    public string InternalName => Encoding.ASCII.GetString(new ArraySegment<byte>(_data, 0x20, 20));
    public string FriendlyName => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(InternalName.ToLowerInvariant());
    public uint CRC1 => RomHelper.AsBigEndian(BitConverter.ToUInt32(_data, 0x0010));
    public uint CRC2 => RomHelper.AsBigEndian(BitConverter.ToUInt32(_data, 0x0014));
    public byte Version => _data[0x3F];
    public string FileName => Path.GetFileName(_path);
    
    internal readonly string _path;
    private readonly byte[] _data;
    
    
}