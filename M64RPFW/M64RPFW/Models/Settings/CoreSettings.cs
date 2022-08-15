using System;
using M64RPFW.Models.Emulation.Core;

namespace M64RPFW.Models.Settings;

public class CoreSettings
{
    private const string SECTION_NAME = "Core";
    
    public CoreSettings()
    {
        _handle = Mupen64Plus.ConfigOpenSection(SECTION_NAME);
    }
    
    public enum EmulatorType
    {
        PureInterpreter = 0,
        CachedInterpreter = 1,
        DynamicRecompiler = 2
    }

    public EmulatorType R4300Emulator
    {
        get => (EmulatorType) Mupen64Plus.ConfigGetInt(_handle, "R4300Emulator");
        set => Mupen64Plus.ConfigSetInt(_handle, "R4300Emulator", (int) value);
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
    

    private readonly IntPtr _handle;
}