using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;

namespace M64RPFW.ViewModels
{
    public partial class ROMViewModel : ObservableObject
    {
        private readonly ROM rom;

        public string Path { get; }

        public bool IsValid => rom.IsValid;
        public bool IsBigEndian => rom.IsBigEndian;
        public string InternalName => rom.InternalName;
        public string FriendlyName => rom.FriendlyName;
        public uint PrimaryCRC => rom.PrimaryCRC;
        public uint SecondaryCRC => rom.SecondaryCRC;
        public uint MediaFormat => rom.MediaFormat;
        public byte CountryCode => rom.CountryCode;
        public byte Version => rom.Version;

        public ROMViewModel(byte[] data, string path)
        {
            rom = new(data);
            Path = path;

            OnPropertyChanged(); // just notify that this entire vm changed
        }

        public override string ToString()
        {
            return rom.FriendlyName;
        }
    }
}
