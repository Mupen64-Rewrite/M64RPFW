using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Core.Emulation.ROM;
using M64RPFW.src.Helpers;
using System;
using System.Globalization;
using System.IO;

namespace M64RPFW.UI.ViewModels
{
    public partial class ROMViewModel : ObservableObject
    {
        private ROM rom;

        public string Path
        {
            get => rom.Path; set => SetProperty(ref rom.Path, value);
        }

        public bool IsValid => rom.IsValid;
        public bool IsBigEndian => rom.IsBigEndian;
        public string FriendlyName => rom.FriendlyName;
        public uint PrimaryCRC => rom.PrimaryCRC;
        public uint SecondaryCRC => rom.SecondaryCRC;
        public uint MediaFormat => rom.MediaFormat;
        public byte CountryCode => rom.CountryCode;
        public byte Version => rom.Version;


        public ROMViewModel(string _path)
        {
            rom = new(_path, File.ReadAllBytes(_path));

            if (!rom.IsBigEndian)
            {
                // byteswap to avoid display issues
                // e.g.: Usep Ramir O64 -> Super Mario 64 
                ROMHelper.Byteswap(ref rom.RawData);
            }

            OnPropertyChanged(); // notify due to byteswap
        }

        public override string ToString()
        {
            return rom.FriendlyName;
        }
    }
}
