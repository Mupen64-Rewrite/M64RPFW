using System.Runtime.InteropServices;
using System.Text;

namespace M64RPFW.Models.Movies;

public class MovieHeader
{
    public MovieHeader()
    {
        _data = new();
    }

    public unsafe void Load(string path)
    {
        using var rd = new BinaryReader(File.OpenRead(path));
        var buffer = rd.ReadBytes(1024);

        fixed (byte* pBuffer = buffer)
        {
            _data = Marshal.PtrToStructure<Data>((IntPtr) pBuffer);
        }
    }

    public unsafe void Save(string path)
    {
        using var wr = new BinaryWriter(File.OpenWrite(path));
        var buffer = new byte[1024];

        fixed (byte* pBuffer = buffer)
        {
            Marshal.StructureToPtr(_data, (IntPtr) pBuffer, false);
        }
        
        wr.Write(buffer);
    }
    
    [StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Ansi)]
    private unsafe struct Data
    {
        static Data()
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new PlatformNotSupportedException(
                    "I don't know how to tell you this... your computer is SUPER old.");
            }
        }
        
        [FieldOffset(0x000)]
        public uint Magic;
        [FieldOffset(0x004)]
        public uint Version;
        [FieldOffset(0x008)]
        public int Uid;

        [FieldOffset(0x00C)]
        public uint VICount;
        [FieldOffset(0x010)]
        public uint RerecordCount;
        [FieldOffset(0x014)]
        public byte VisPerSecond;
        [FieldOffset(0x015)]
        public byte Controllers;
        [FieldOffset(0x018)]
        public uint SampleCount;

        [FieldOffset(0x01C)]
        public ushort StartType;
        [FieldOffset(0x020)]
        public byte CtrlsPresent;
        [FieldOffset(0x021)]
        public byte CtrlMemPaks;
        [FieldOffset(0x022)]
        public byte CtrlRumblePaks;
        
        [FieldOffset(0x0C4)]
        public fixed byte RomName[32];
        [FieldOffset(0x0E4)]
        public uint RomCRC;
        [FieldOffset(0x0E8)]
        public ushort CountryCode;
        [FieldOffset(0x122)]
        public fixed byte VideoPlugin[64];
        [FieldOffset(0x162)]
        public fixed byte AudioPlugin[64];
        [FieldOffset(0x1A2)]
        public fixed byte InputPlugin[64];
        [FieldOffset(0x1E2)]
        public fixed byte RspPlugin[64];
        [FieldOffset(0x222)]
        public fixed byte Authors[222];
        [FieldOffset(0x300)]
        public fixed byte Description[256];
    }

    private static unsafe void CopyStringToPtr(string s, byte* p, int maxLen, Encoding? enc = null)
    {
        if (enc == null)
            enc = Encoding.UTF8;
        s = (s.Length > (maxLen - 1) ? s[..(maxLen - 1)] : s) + '\0';
        enc.GetBytes(s, new Span<byte>(p, maxLen));
    }

    private Data _data;

    public int Uid
    {
        get => _data.Uid;
        set => _data.Uid = value;
    }

    public uint VICount
    {
        get => _data.VICount;
        set => _data.VICount = value;
    }

    public uint RerecordCount
    {
        get => _data.RerecordCount;
        set => _data.RerecordCount = value;
    }

    public byte VisPerSecond
    {
        get => _data.VisPerSecond;
        set => _data.VisPerSecond = value;
    }

    public byte Controllers
    {
        get => _data.Controllers;
        set => _data.Controllers = value;
    }

    public uint SampleCount
    {
        get => _data.SampleCount;
        set => _data.SampleCount = value;
    }

    public ushort StartType
    {
        get => _data.StartType;
        set => _data.StartType = value;
    }

    public byte CtrlsPresent
    {
        get => _data.CtrlsPresent;
        set => _data.CtrlsPresent = value;
    }

    public byte CtrlMemPaks
    {
        get => _data.CtrlMemPaks;
        set => _data.CtrlMemPaks = value;
    }

    public byte CtrlRumblePaks
    {
        get => _data.CtrlRumblePaks;
        set => _data.CtrlRumblePaks = value;
    }

    public unsafe string RomName
    {
        get
        {
            fixed (byte* romName = _data.RomName)
            {
                return Marshal.PtrToStringAnsi((IntPtr) romName)!;
            }
        }
        set
        {
            fixed (byte* romName = _data.RomName)
            {
                CopyStringToPtr(value, romName, 32, Encoding.ASCII);
            }
        }
    }

    public uint RomCRC
    {
        get => _data.RomCRC;
        set => _data.RomCRC = value;
    }

    public ushort CountryCode
    {
        get => _data.CountryCode;
        set => _data.CountryCode = value;
    }

    public unsafe string VideoPlugin
    {
        get
        {
            fixed (byte* p = _data.VideoPlugin)
            {
                return Marshal.PtrToStringAnsi((IntPtr) p)!;
            }
        }
        set
        {
            fixed (byte* p = _data.VideoPlugin)
            {
                CopyStringToPtr(value, p, 64, Encoding.ASCII);
            }
        }
    }

    public unsafe string AudioPlugin
    {
        get
        {
            fixed (byte* p = _data.AudioPlugin)
            {
                return Marshal.PtrToStringAnsi((IntPtr) p)!;
            }
        }
        set
        {
            fixed (byte* p = _data.AudioPlugin)
            {
                CopyStringToPtr(value, p, 64, Encoding.ASCII);
            }
        }
    }

    public unsafe string InputPlugin
    {
        get
        {
            fixed (byte* p = _data.InputPlugin)
            {
                return Marshal.PtrToStringAnsi((IntPtr) p)!;
            }
        }
        set
        {
            fixed (byte* p = _data.InputPlugin)
            {
                CopyStringToPtr(value, p, 64, Encoding.ASCII);
            }
        }
    }

    public unsafe string RspPlugin
    {
        get
        {
            fixed (byte* p = _data.RspPlugin)
            {
                return Marshal.PtrToStringAnsi((IntPtr) p)!;
            }
        }
        set
        {
            fixed (byte* p = _data.RspPlugin)
            {
                CopyStringToPtr(value, p, 64, Encoding.ASCII);
            }
        }
    }

    public unsafe string Authors
    {
        get
        {
            fixed (byte* p = _data.Authors)
            {
                return Marshal.PtrToStringAnsi((IntPtr) p)!;
            }
        }
        set
        {
            fixed (byte* p = _data.Authors)
            {
                CopyStringToPtr(value, p, 222, Encoding.UTF8);
            }
        }
    }

    public unsafe string Description
    {
        get
        {
            fixed (byte* p = _data.Description)
            {
                return Marshal.PtrToStringAnsi((IntPtr) p)!;
            }
        }
        set
        {
            fixed (byte* p = _data.Description)
            {
                CopyStringToPtr(value, p, 256, Encoding.UTF8);
            }
        }
    }
}