﻿using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Models.Emulation;

namespace M64RPFW.ViewModels;

public class RomViewModel : ObservableObject
{
    private readonly Rom _rom;

    public RomViewModel(byte[] data, string path)
    {
        _rom = new Rom(data);
        Path = path;
    }

    public string Path { get; }

    public byte[] RawData => _rom.RawData;
    public bool IsValid => _rom.IsValid;
    public bool IsBigEndian => _rom.IsBigEndian;
    public string InternalName => _rom.InternalName;
    public string FriendlyName => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(InternalName.Trim().ToLowerInvariant());
    public uint PrimaryCrc => _rom.PrimaryCrc;
    public uint SecondaryCrc => _rom.SecondaryCrc;
    public uint MediaFormat => _rom.MediaFormat;
    public byte CountryCode => _rom.CountryCode;
    public byte Version => _rom.Version;

    public override string ToString()
    {
        return $"{FriendlyName} ({CountryCode})";
    }
}