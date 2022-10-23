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
        private ROM rom;

        public string Path
        {
            get => rom.Path; set => SetProperty(ref rom.Path, value);
        }

        public bool IsValid => rom.IsValid;
        public bool IsBigEndian => rom.IsBigEndian;
        public string InternalName => rom.InternalName;
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

        [RelayCommand]
        private void ShowInspectionWindow(ROMViewModel rom) => new ROMInspectionWindow() { DataContext = this }.Show();
    }
}
