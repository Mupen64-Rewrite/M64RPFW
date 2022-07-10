using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation.Helper;
using System;
using System.Globalization;
using System.IO;

namespace M64RPFW.UI.ViewModels
{
    public partial class ROMViewModel : ObservableObject
    {
        [ObservableProperty]
        private string path;

        [ObservableProperty]
        private byte[] rawData;

        public bool IsBigEndian => RawData[0] == 0x80;
        public string InternalName => ROMHelper.GetInternalName(IsBigEndian, RawData);
        public string FriendlyName => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(InternalName.ToLowerInvariant());
        public bool IsValid => RawData.Length >= 0x0FFF && ROMHelper.HasValidHeader(IsBigEndian, RawData);
        public uint PrimaryCRC => BitConverter.ToUInt32(new ArraySegment<byte>(rawData, 0x0010, sizeof(UInt32)).ToArray());
        public uint SecondaryCRC => BitConverter.ToUInt32(new ArraySegment<byte>(rawData, 0x0014, sizeof(UInt32)).ToArray());
        public uint MediaFormat => BitConverter.ToUInt32(new ArraySegment<byte>(rawData, 0x0038, sizeof(UInt32)).ToArray());
        public byte CountryCode => rawData[0x003E];
        public byte Version => rawData[0x003F];

        public ROMViewModel(string _path)
        {
            Path = _path;
            RawData = File.ReadAllBytes(Path);

            if (!IsBigEndian)
            {
                // byteswap to avoid display issues
                // e.g.: Usep Ramir O64 -> Super Mario 64 
                ROMHelper.Byteswap(ref rawData);
            }

            OnPropertyChanged(); // notify due to byteswap
        }
    }
}
