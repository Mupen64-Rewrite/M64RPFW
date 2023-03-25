using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using Tomlyn.Model;

namespace M64RPFW.Models.Settings;
using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64PlusTypes.MessageLevel;

/// <summary>
/// Settings handler for the M64RPFW frontend.
/// </summary>
public static class RPFWSettings
{
    private static string? _cfgPath;
    private static string CfgPath => _cfgPath ??= Path.Join(MupenSettings.GetUserConfigPath(), "m64rpfw.json");

    public class SettingsObject : ITomlMetadataProvider
    {
        // storage for comments and whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        
        public string? VideoPluginPath { get; set; }
        public string? AudioPluginPath { get; set; }
        public string? InputPluginPath { get; set; }
        public string? RspPluginPath { get; set; }
    }
}