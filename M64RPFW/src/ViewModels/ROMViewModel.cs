using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Models.Emulation;
using M64RPFW.src.Helpers;
using M64RPFW.UI.Views;
using System.IO;

namespace M64RPFW.UI.ViewModels
{
    public partial class ROMViewModel : ObservableObject
    {
        private readonly ROM rom;

        private readonly string path;
        public string Path => path;

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
            this.path = path;

            OnPropertyChanged(); // just notify that this entire vm changed
        }

        public override string ToString()
        {
            return rom.FriendlyName;
        }

        [RelayCommand]
        private void ShowInspectionWindow(ROMViewModel rom) => new ROMInspectionWindow() { DataContext = this }.Show();
    }
}
