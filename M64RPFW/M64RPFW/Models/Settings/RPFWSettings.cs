using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Tomlyn.Model;

namespace M64RPFW.Models.Settings;

/// <summary>
/// TOML config file model.
/// </summary>
public class RPFWSettings : ITomlMetadataProvider
{
    // Required to maintain comments/whitespace
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    public RPFWSettings()
    {
        Plugins = new PluginsSection();
        Roms = new RomsSection();
    }

    public class PluginsSection : ITomlMetadataProvider
    {
        // Required to maintain comments/whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

        public PluginsSection()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Video = "/usr/lib/mupen64plus/mupen64plus-video-rice.so";
                Audio = "/usr/lib/mupen64plus/mupen64plus-audio-sdl.so";
                Input = "/usr/lib/mupen64plus/mupen64plus-input-sdl.so";
                RSP = "/usr/lib/mupen64plus/mupen64plus-rsp-hle.so";
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string execPath = Directory.GetParent(Assembly.GetEntryAssembly()!.Location)!.FullName;

                Video = $"{execPath}/Libraries/mupen64plus-video-rice.dll";
                Audio = $"{execPath}/Libraries/mupen64plus-audio-sdl.dll";
                Input = $"{execPath}/Libraries/mupen64plus-input-sdl.dll";
                RSP = $"{execPath}/Libraries/mupen64plus-rsp-hle.dll";
                return;
            }

            throw new NotSupportedException("MacOS and other non-Linux Unices are not supported yet");
        }
        public string Video { get; set; }
        public string Audio { get; set; }
        public string Input { get; set; }
        [DataMember(Name = "rsp")] public string RSP { get; set; }
    }
    public PluginsSection Plugins { get; set; }

    public class RomsSection : ITomlMetadataProvider
    {
        // Required to maintain comments/whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

        public RomsSection()
        {
            SearchDir = null;
            Recent = new List<string>();
        }
        
        public string? SearchDir { get; set; }
        public List<string> Recent { get; }
    }
    
    public RomsSection Roms { get; }

}