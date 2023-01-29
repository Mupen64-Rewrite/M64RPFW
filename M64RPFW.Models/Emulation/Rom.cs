using System.Globalization;
using System.Text;

namespace M64RPFW.Models.Emulation;

public class Rom
{
    public readonly byte[] RawData;

    public Rom(byte[] rawData)
    {
        RawData = rawData;

        if (!IsBigEndian)
            // perform LE->BE byteswap
            // e.g.: Usep Ramir O64 -> Super Mario 64 
            Byteswap(ref rawData);
    }

    public bool IsBigEndian => RawData[0] == 0x80;
    public string InternalName => Encoding.ASCII.GetString(new ArraySegment<byte>(RawData, 0x20, 20));

    public string FriendlyName =>
        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(InternalName.Trim().ToLowerInvariant());

    public bool IsValid => RawData.Length is >= 0x0FFF and >= 0x33 && (IsBigEndian ? RawData[0] == 0x80 : RawData[0] == 0x37);
    public uint PrimaryCrc => BitConverter.ToUInt32(new ArraySegment<byte>(RawData, 0x0010, sizeof(uint)).ToArray());
    public uint SecondaryCrc => BitConverter.ToUInt32(new ArraySegment<byte>(RawData, 0x0014, sizeof(uint)).ToArray());
    public uint MediaFormat => BitConverter.ToUInt32(new ArraySegment<byte>(RawData, 0x0038, sizeof(uint)).ToArray());
    public byte CountryCode => RawData[0x003E];
    public byte Version => RawData[0x003F];

    private static void Byteswap(ref byte[] data)
    {
        for (var i = 0; i < data.Length / 2; i++) (data[i * 2 + 1], data[i * 2]) = (data[i * 2], data[i * 2 + 1]);
    }
}