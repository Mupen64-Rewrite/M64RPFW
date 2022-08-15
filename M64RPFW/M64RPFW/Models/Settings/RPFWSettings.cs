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
        RecentRoms = new List<string>();
    }

    public class PluginsSection : ITomlMetadataProvider
    {
        // Required to maintain comments/whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

        public PluginsSection()
        {
            Console.WriteLine($"Is Linux: {RuntimeInformation.IsOSPlatform(OSPlatform.Linux)}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SearchDir = "/usr/lib/mupen64plus";
                Video = "mupen64plus-video-rice.so";
                Audio = "mupen64plus-audio-sdl.so";
                Input = "mupen64plus-input-sdl.so";
                RSP = "mupen64plus-rsp-hle.so";
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string execPath = Directory.GetParent(Assembly.GetEntryAssembly()!.Location)!.FullName;

                SearchDir = Path.Join(execPath, "Libraries");
                Video = "mupen64plus-video-rice.dll";
                Audio = "mupen64plus-audio-sdl.dll";
                Input = "mupen64plus-input-sdl.dll";
                RSP = "mupen64plus-rsp-hle.dll";
                return;
            }

            throw new NotSupportedException("MacOS and other non-Linux Unices are not supported yet");
        }

        public string SearchDir { get; set; }
        public string Video { get; set; }
        public string Audio { get; set; }
        public string Input { get; set; }
        [DataMember(Name = "rsp")] public string RSP { get; set; }
    }

    public PluginsSection Plugins { get; set; }
    
    public List<string> RecentRoms { get; }

}