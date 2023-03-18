using System;
using System.IO;

namespace Mupen64PlusRR.Models.Helpers;

public static class RomHelper
{
    public static void AdaptiveByteSwap(ref byte[] data)
    {
        // read signature (0x00, 4 bytes) in big-endian order
        uint signature = data[3] | ((uint) data[2] << 8) | ((uint) data[1] << 16) | ((uint) data[0] << 24);
            
        switch (signature)
        {
            // Correct byte ordering
            case 0x80_37_12_40:
                return;
            // Swap every 2 bytes
            case 0x37_80_40_12:
                for (int i = 0; i < data.Length; i += 2)
                {
                    (data[i + 0], data[i + 1]) = (data[i + 1], data[i + 0]);
                }

                return;
            // Swap every 4 bytes
            case 0x40_12_37_80:
                for (int i = 0; i < data.Length; i += 4)
                {
                    (data[i + 0], data[i + 1], data[i + 2], data[i + 3]) =
                        (data[i + 3], data[i + 2], data[i + 1], data[i + 0]);
                }
                return;
        }

        throw new ArgumentException($"Data is most likely not an N64 ROM. Signature was {signature:X8}");
    }

    public static byte[] ReadFileSection(string path, long begin, long end)
    {
        byte[] data = new byte[end - begin];
        using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
        {
            file.Seek(begin, SeekOrigin.Begin);
            if (file.Read(data) < data.LongLength)
                throw new InvalidOperationException("Hit EOF while trying to read bytes");
        }

        return data;
    }

    public static uint AsBigEndian(uint x)
    {
        if (BitConverter.IsLittleEndian) {
            x = (x >> 16) | (x << 16);
            x = ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }
        return x;
    }
}