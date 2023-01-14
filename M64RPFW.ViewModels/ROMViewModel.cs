using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;

namespace M64RPFW.ViewModels;

public class RomViewModel : ObservableObject
{
    private readonly Rom rom;

    public RomViewModel(byte[] data, string path)
    {
        rom = new Rom(data);
        Path = path;

        OnPropertyChanged(); // just notify that this entire vm changed
    }

    public string Path { get; }

    public byte[] RawData => rom.RawData;
    public bool IsValid => rom.IsValid;
    public bool IsBigEndian => rom.IsBigEndian;
    public string InternalName => rom.InternalName;
    public string FriendlyName => rom.FriendlyName;
    public uint PrimaryCRC => rom.PrimaryCRC;
    public uint SecondaryCRC => rom.SecondaryCRC;
    public uint MediaFormat => rom.MediaFormat;
    public byte CountryCode => rom.CountryCode;
    public byte Version => rom.Version;

    public override string ToString()
    {
        return rom.FriendlyName;
    }
}