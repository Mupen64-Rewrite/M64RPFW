using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.src.Models.Emulation.Core.API
{
    public class Mupen64PlusLaunchParameters
    {
        public readonly byte[] Rom;
        public readonly Mupen64PlusConfig Config;
        public readonly int InitialSlot;
        public readonly string CoreLibraryPath;
        public readonly string VideoPluginPath;
        public readonly string AudioPluginPath;
        public readonly string InputPluginPath;
        public readonly string RSPPluginPath;

        public Mupen64PlusLaunchParameters(byte[] rom, Mupen64PlusConfig config, int initialSlot, string coreLibraryPath, string videoPluginPath, string audioPluginPath, string inputPluginPath, string rSPPluginPath)
        {
            Rom = rom;
            Config = config;
            InitialSlot = initialSlot;
            VideoPluginPath = videoPluginPath;
            AudioPluginPath = audioPluginPath;
            InputPluginPath = inputPluginPath;
            RSPPluginPath = rSPPluginPath;
            CoreLibraryPath = coreLibraryPath;
        }
    }
}
