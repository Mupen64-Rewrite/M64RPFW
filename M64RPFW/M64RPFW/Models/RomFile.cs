using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
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
        _path = Path.GetFullPath(path);
        Console.WriteLine($"Loading ROM from {_path}");
        
        // Read ROM header
        _header = RomHelper.ReadFileSection(_path, 0, 64);
        RomHelper.AdaptiveByteSwap(ref _header);
    }
    
    public RegionEnum Region => _header[0x3E] switch
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

    public unsafe void LoadThisRom()
    {
        FileInfo fi = new FileInfo(_path);
        if (fi.Length < 0x1000)
            throw new InvalidOperationException("Data is too short to be a ROM!");
        // 2^20 = 1 MiB, 2^26 = 64 MiB
        if (fi.Length >= 1 << 26)
            throw new InvalidOperationException("Data is too large for a cartridge!");
        
        IntPtr blockPtr = Marshal.AllocHGlobal((nint) fi.Length);
        Span<byte> block = new((void*) blockPtr, (int) fi.Length);
        try
        {
            using var file = File.OpenRead(_path);
            Trace.Assert(file.Read(block) == fi.Length);
            
            Mupen64Plus.OpenRomBinary(block);
        }
        finally
        {
            Marshal.FreeHGlobal(blockPtr);
        }
    }

    public string InternalName => Encoding.ASCII.GetString(new ArraySegment<byte>(_header, 0x20, 20));
    public string FriendlyName => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(InternalName.ToLowerInvariant());
    public uint CRC1 => RomHelper.AsBigEndian(BitConverter.ToUInt32(_header, 0x0010));
    public uint CRC2 => RomHelper.AsBigEndian(BitConverter.ToUInt32(_header, 0x0014));
    public byte Version => _header[0x3F];
    public string FileName => Path.GetFileName(_path);
    
    internal readonly string _path;
    private readonly byte[] _header;
    
    
}