using M64RPFW.src.Models.Emulation.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static M64RPFW.Models.Emulation.Core.API.Mupen64PlusTypes;

namespace M64RPFW.src.Models.Emulation.Core.API
{
    public struct Mupen64PlusConfigEntry<T>
    {
        public readonly string Section;
        public readonly string Name;
        public T Value { get; set; } = default;

        public Mupen64PlusConfigEntry(string section, string name)
        {
            Section = section;
            Name = name;
        }

        public Mupen64PlusConfigEntry(string section, string name, T value)
        {
            Section = section;
            Name = name;
            Value = value;
        }
    }

    public struct Mupen64PlusConfig
    {
        public Mupen64PlusConfigEntry<int> CoreType = new("Core", "R4300Emulator");
        public Mupen64PlusConfigEntry<bool> NoCompiledJump = new("Core", "NoCompiledJump");
        public Mupen64PlusConfigEntry<bool> DisableExtraMemory = new("Core", "DisableExtraMem");
        public Mupen64PlusConfigEntry<bool> DelaySpecialInterrupt = new("Core", "DelaySI");
        public Mupen64PlusConfigEntry<int> CyclesPerOp = new("Core", "CountPerOp");
        public Mupen64PlusConfigEntry<bool> DisableSpecialRecompilation = new("Core", "DisableSpecRecomp");
        public Mupen64PlusConfigEntry<bool> RandomizeInterrupt = new("Core", "RandomizeInterrupt");

        public Mupen64PlusConfigEntry<int> ScreenWidth = new("Video-General", "ScreenWidth");
        public Mupen64PlusConfigEntry<int> ScreenHeight = new("Video-General", "ScreenHeight");
        public Mupen64PlusConfigEntry<bool> VerticalSynchronization = new("Video-General", "VerticalSync");
        public Mupen64PlusConfigEntry<bool> OnScreenDisplay = new("Video-General", "OnScreenDisplay");

        public Mupen64PlusConfig()
        {

        }

    }
}
