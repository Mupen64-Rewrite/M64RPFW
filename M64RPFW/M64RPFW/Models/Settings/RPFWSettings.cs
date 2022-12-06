using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using M64RPFW.Models.Helpers;
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
            string execPath = Directory.GetParent(Assembly.GetEntryAssembly()!.Location)!.FullName;
            Video = NativeLibHelper.AsDLL($"{execPath}/Libraries/mupen64plus-video-rice");
            Audio = NativeLibHelper.AsDLL($"{execPath}/Libraries/mupen64plus-audio-sdl");
            Input = NativeLibHelper.AsDLL($"{execPath}/Libraries/mupen64plus-input-sdl");
            RSP = NativeLibHelper.AsDLL($"{execPath}/Libraries/mupen64plus-rsp-hle");
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