using M64RPFW.Core.Emulation.Exceptions;
using M64RPFW.Models.Emulation.Core;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace M64RPFW.src.Helpers
{
    public static class ROMHelper
    {
        // Assumes native .z64
        public static string GetInternalName(bool bigEndian, byte[] rom)
        {
            if (rom.Length < 0x33) throw new ROMException("ROM is too short");
            return
            new Regex(@"\s{2,}").Replace(Encoding.ASCII.GetString(new ArraySegment<byte>(rom, 0x20, 0x33 - 0x20)), "").Trim();
        }

        public static bool HasValidHeader(bool bigEndian, byte[] rom)
        {
            if (rom.Length < 0x33) return false;

            if (bigEndian)
                return rom[0] == 0x80;
            else
                return rom[0] == 0x37;
        }

        public static void Byteswap(ref byte[] rom)
        {
            for (int i = 0; i < rom.Length / 2; i++)
            {
                byte tmp = rom[i * 2];
                rom[i * 2] = rom[i * 2 + 1];
                rom[i * 2 + 1] = tmp;
            }
        }

    }
}
