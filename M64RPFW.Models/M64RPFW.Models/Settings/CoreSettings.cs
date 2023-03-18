using M64RPFW.Models.Emulation;

// ReSharper disable UnusedMember.Global

namespace M64RPFW.Models.Settings;

public class CoreSettings
{
    public enum EmulatorType
    {
        PureInterpreter = 0,
        CachedInterpreter = 1,
        DynamicRecompiler = 2
    }

    private const string SECTION_NAME = "Core";


    private readonly nint _handle;

    public CoreSettings()
    {
        _handle = Mupen64Plus.ConfigOpenSection(SECTION_NAME);
    }

    public EmulatorType R4300Emulator
    {
        get => (EmulatorType)Mupen64Plus.ConfigGetInt(_handle, "R4300Emulator");
        set => Mupen64Plus.ConfigSetInt(_handle, "R4300Emulator", (int)value);
    }

    public bool DisableExtraMem
    {
        get => Mupen64Plus.ConfigGetBool(_handle, "DisableExtraMem");
        set => Mupen64Plus.ConfigSetBool(_handle, "DisableExtraMem", value);
    }

    public bool RandomizeInterrupt
    {
        get => Mupen64Plus.ConfigGetBool(_handle, "RandomizeInterrupt");
        set => Mupen64Plus.ConfigSetBool(_handle, "RandomizeInterrupt", value);
    }

    public bool AutoStateSlotIncrement
    {
        get => Mupen64Plus.ConfigGetBool(_handle, "AutoStateSlotIncrement");
        set => Mupen64Plus.ConfigSetBool(_handle, "AutoStateSlotIncrement", value);
    }

    private static void ClearCoreEvents()
    {
        const string eventsSection = "CoreEvents";

        var handle = Mupen64Plus.ConfigOpenSection(eventsSection);

        foreach (var (name, _) in Mupen64Plus.ConfigListParameters(handle))
            if (name != "Version")
                Mupen64Plus.ConfigSetString(handle, name, "");
        Mupen64Plus.ConfigSaveSection(eventsSection);
    }
}