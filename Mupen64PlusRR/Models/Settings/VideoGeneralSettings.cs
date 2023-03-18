using System;
using Mupen64PlusRR.Models.Emulation;

namespace Mupen64PlusRR.Models.Settings;

public class VideoGeneralSettings
{
    private const string SECTION_NAME = "Video-General";
    
    public VideoGeneralSettings()
    {
        _handle = Mupen64Plus.ConfigOpenSection(SECTION_NAME);
    }
    
    public enum Rotation
    {
        Landscape = 0,
        Portrait,
        LandscapeInverted,
        PortraitInverted
    }
    
    public int ScreenWidth
    {
        get => Mupen64Plus.ConfigGetInt(_handle, "ScreenWidth");
        set => Mupen64Plus.ConfigSetInt(_handle, "ScreenWidth", value);
    }
    
    public int ScreenHeight
    {
        get => Mupen64Plus.ConfigGetInt(_handle, "ScreenHeight");
        set => Mupen64Plus.ConfigSetInt(_handle, "ScreenHeight", value);
    }
    
    public bool VerticalSync
    {
        get => Mupen64Plus.ConfigGetBool(_handle, "VerticalSync");
        set => Mupen64Plus.ConfigSetBool(_handle, "VerticalSync", value);
    }
    

    private readonly IntPtr _handle;
}