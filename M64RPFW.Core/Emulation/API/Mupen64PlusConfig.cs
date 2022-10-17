using M64RPFW.src.Models.Emulation.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static M64RPFW.Models.Emulation.Core.API.Mupen64PlusTypes;

namespace M64RPFW.src.Models.Emulation.Core.API
{
    public struct Mupen64PlusConfigEntry
    {
        public readonly string Section;
        public readonly string Name;
        public object Value { get; set; } = default;

        public Mupen64PlusConfigEntry(string section, string name)
        {
            Section = section;
            Name = name;
        }

        public Mupen64PlusConfigEntry(string section, string name, object value)
        {
            Section = section;
            Name = name;
            Value = value;
        }
    }

    public struct Mupen64PlusConfig
    {
        public Mupen64PlusConfigEntry CoreType = new("Core", "R4300Emulator");
        public Mupen64PlusConfigEntry NoCompiledJump = new("Core", "NoCompiledJump");
        public Mupen64PlusConfigEntry DisableExtraMemory = new("Core", "DisableExtraMem");
        public Mupen64PlusConfigEntry DelaySpecialInterrupt = new("Core", "DelaySI");
        public Mupen64PlusConfigEntry CyclesPerOp = new("Core", "CountPerOp");
        public Mupen64PlusConfigEntry DisableSpecialRecompilation = new("Core", "DisableSpecRecomp");
        public Mupen64PlusConfigEntry RandomizeInterrupt = new("Core", "RandomizeInterrupt");

        public Mupen64PlusConfigEntry ScreenWidth = new("Video-General", "ScreenWidth");
        public Mupen64PlusConfigEntry ScreenHeight = new("Video-General", "ScreenHeight");
        public Mupen64PlusConfigEntry VerticalSynchronization = new("Video-General", "VerticalSync");
        public Mupen64PlusConfigEntry OnScreenDisplay = new("Video-General", "OnScreenDisplay");

        public Mupen64PlusConfig()
        {

        }

    }
}
