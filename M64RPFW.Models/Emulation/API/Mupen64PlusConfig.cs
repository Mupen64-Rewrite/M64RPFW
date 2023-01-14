namespace M64RPFW.Models.Emulation.API;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal sealed class Mupen64PlusConfigEntryAttribute : Attribute
{
    internal readonly string Name;
    internal readonly string Section;

    public Mupen64PlusConfigEntryAttribute(string section, string name)
    {
        Section = section;
        Name = name;
    }
}

public readonly struct Mupen64PlusConfig
{
    [Mupen64PlusConfigEntry("Core", "R4300Emulator")]
    public readonly int CoreType;

    [Mupen64PlusConfigEntry("Video-General", "ScreenWidth")]
    public readonly int ScreenWidth;

    [Mupen64PlusConfigEntry("Video-General", "ScreenHeight")]
    public readonly int ScreenHeight;

    public Mupen64PlusConfig(int coreType, int screenWidth, int screenHeight)
    {
        CoreType = coreType;
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
    }
}