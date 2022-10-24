﻿using M64RPFW.Models.Emulation.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M64RPFW.Models.Emulation
{
    public class ROM
    {
        public readonly byte[] RawData;

        public ROM(byte[] rawData)
        {
            RawData = rawData;

            if (!IsBigEndian)
            {
                // perform LE->BE byteswap
                // e.g.: Usep Ramir O64 -> Super Mario 64 
                Byteswap(ref rawData);
            }
        }

        public bool IsBigEndian => RawData[0] == 0x80;
        public string InternalName => Encoding.ASCII.GetString(new ArraySegment<byte>(RawData, 0x20, 20));
        public string FriendlyName => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(InternalName.Trim().ToLowerInvariant());
        public bool IsValid => RawData.Length >= 0x0FFF && HasValidHeader(IsBigEndian, RawData);
        public uint PrimaryCRC => BitConverter.ToUInt32(new ArraySegment<byte>(RawData, 0x0010, sizeof(uint)).ToArray());
        public uint SecondaryCRC => BitConverter.ToUInt32(new ArraySegment<byte>(RawData, 0x0014, sizeof(uint)).ToArray());
        public uint MediaFormat => BitConverter.ToUInt32(new ArraySegment<byte>(RawData, 0x0038, sizeof(uint)).ToArray());
        public byte CountryCode => RawData[0x003E];
        public byte Version => RawData[0x003F];

        private static bool HasValidHeader(bool bigEndian, byte[] rom)
        {
            if (rom.Length < 0x33) return false;

            if (bigEndian)
                return rom[0] == 0x80;
            else
                return rom[0] == 0x37;
        }

        private static void Byteswap(ref byte[] data)
        {
            for (int i = 0; i < data.Length / 2; i++)
            {
                byte tmp = data[i * 2];
                data[i * 2] = data[i * 2 + 1];
                data[i * 2 + 1] = tmp;
            }
        }
    }
}
