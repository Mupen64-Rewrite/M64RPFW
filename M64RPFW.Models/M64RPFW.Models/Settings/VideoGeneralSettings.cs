using M64RPFW.Models.Emulation;

namespace M64RPFW.Models.Settings;

public class VideoGeneralSettings
{
    public enum Rotation
    {
        Landscape = 0,
        Portrait,
        LandscapeInverted,
        PortraitInverted
    }

    private const string SECTION_NAME = "Video-General";


    private readonly nint _handle;

    public VideoGeneralSettings()
    {
        _handle = Mupen64Plus.ConfigOpenSection(SECTION_NAME);
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
}