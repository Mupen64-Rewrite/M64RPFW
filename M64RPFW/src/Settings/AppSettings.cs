using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.src.Settings
{
    internal sealed class AppSettings
    {
        #region Emulation

        public int CoreType { get; set; } = 0;
        public string CoreLibraryPath { get; set; } = "m64p/mupen64plus.dll";
        public string VideoPluginPath { get; set; } = "m64p/mupen64plus-video-rice.dll";
        public string AudioPluginPath { get; set; } = "m64p/mupen64plus-audio-sdl.dll";
        public string InputPluginPath { get; set; } = "m64p/mupen64plus-input-sdl.dll";
        public string RSPPluginPath { get; set; } = "m64p/mupen64plus-rsp-hle.dll";
        public int DefaultSlot { get; set; } = 0;
        public bool PauseOnFrameAdvance { get; set; } = true;
        public string[] RecentROMPaths { get; set; } = Array.Empty<string>();
        public int ScreenWidth { get; set; } = 800;
        public int ScreenHeight { get; set; } = 600;

        #endregion

        #region Appearance
        public string Culture { get; set; } = "en-US";
        public bool ShowStatusbar { get; set; } = true;
        public string Theme { get; set; } = "Light";

        #endregion



    }
}
