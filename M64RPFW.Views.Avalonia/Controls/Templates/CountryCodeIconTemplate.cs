using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Controls.Templates;

public class CountryCodeIconTemplate : IRecyclingDataTemplate
{
    private static readonly Dictionary<string, IImage> _imageCache = new();

    private static IImage LoadCache(string src)
    {
        if (_imageCache.TryGetValue(src, out var img))
            return img;
        
        img = new SvgImage
        {
            Source = SvgSource.Load(src, null)
        };
        _imageCache[src] = img;
        return img;
    }

    private static string CountryCodeToURI(byte code)
    {
        // codes from http://en64.shoutwiki.com/wiki/ROM
        var file = code switch
        {
            0x00 => "demo.svg",
            0x37 => "beta.svg",
            0x41 => "flag-JP.svg", // en64 wiki calls this "Asian"
            0x42 => "flag-BR.svg",
            0x43 => "flag-CN.svg",
            0x44 => "flag-DE.svg",
            0x45 => "flag-US.svg",
            0x46 => "flag-FR.svg",
            0x47 => "flag-US.svg", // Gateway 64 (NTSC)
            0x48 => "flag-NL.svg",
            0x49 => "flag-IT.svg",
            0x4A => "flag-JP.svg",
            0x4B => "flag-KR.svg",
            0x4C => "flag-EU.svg", // Gateway 64 (PAL)
            0x4E => "flag-CA.svg",
            0x50 => "flag-EU.svg",
            0x53 => "flag-ES.svg",
            0x55 => "flag-AU.svg",
            0x57 => "flag-SE.svg", // en64 wiki calls this "Scandinavian"
            0x58 => "flag-EU.svg",
            0x59 => "flag-EU.svg", // @Aurumaker72 says this is supposed to be Australia
            _ => "unknown.svg"
        };
        return $"avares://M64RPFW.Views.Avalonia/Assets/CountryCodes/{file}";
    }
    
    public Control? Build(object? param)
    {
        return Build(param, null);
    }

    public bool Match(object? data)
    {
        return data is RomBrowserItemViewModel;
    }

    public Control? Build(object? data, Control? existing)
    {
        if (data is not RomBrowserItemViewModel vm)
            return null;

        var img = (Image?) existing ?? new Image();
        img.Source = LoadCache(CountryCodeToURI(vm.CountryCode));
        
        return img;
    }
}