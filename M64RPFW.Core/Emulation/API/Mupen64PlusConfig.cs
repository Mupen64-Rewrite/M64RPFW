using M64RPFW.src.Models.Emulation.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static M64RPFW.Models.Emulation.Core.API.Mupen64PlusTypes;

namespace M64RPFW.src.Models.Emulation.Core.API
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class Mupen64PlusConfigEntryAttribute : Attribute
    {
        internal readonly string Section;
        internal readonly string Name;

        public Mupen64PlusConfigEntryAttribute(string section, string name)
        {
            this.Section = section;
            this.Name = name;
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
}
