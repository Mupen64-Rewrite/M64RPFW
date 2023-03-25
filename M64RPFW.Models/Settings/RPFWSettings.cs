using System.Diagnostics.Contracts;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;
using Tomlyn;
using Tomlyn.Model;

namespace M64RPFW.Models.Settings;
using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64PlusTypes.MessageLevel;

/// <summary>
/// Settings handler for the M64RPFW frontend.
/// </summary>
public class RPFWSettings : ITomlMetadataProvider
{
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    public class PluginsSection : ITomlMetadataProvider
    {
        // storage for comments and whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

        public string VideoPath { get; set; } = $"./Libraries/mupen64plus-video-rice{NativeLibHelper.LibraryExtension}";
        public string AudioPath { get; set; } = $"./Libraries/mupen64plus-audio-sdl{NativeLibHelper.LibraryExtension}";
        public string InputPath { get; set; } = $"./Libraries/mupen64plus-input-sdl{NativeLibHelper.LibraryExtension}";
        public string RspPath { get; set; } = $"./Libraries/mupen64plus-rsp-hle{NativeLibHelper.LibraryExtension}";
    }

    public PluginsSection Plugins { get; } = new();

    public class GeneralSection : ITomlMetadataProvider
    {
        // storage for comments and whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

        public List<string> RecentRoms { get; } = new();
        public string Locale { get; set; } = "en_US";
        public bool IsStatusBarVisible { get; set; } = true;
    }

    public GeneralSection General { get; } = new();


    #region Singleton definition

    private static string? _cfgPath;
    private static string CfgPath => _cfgPath ??= Path.Join(MupenSettings.GetUserConfigPath(), "m64rpfw.toml");
    
    public static RPFWSettings Instance { get; private set; }

    static RPFWSettings()
    {
        Load();
    }

    public static void Load()
    {
        if (!File.Exists(CfgPath))
        {
            File.Create(CfgPath).Close();
            Instance = new RPFWSettings();
        }
        else
        {
            string text = File.ReadAllText(CfgPath, Encoding.UTF8);
            Instance = Toml.ToModel<RPFWSettings>(text, CfgPath);
        }
    }

    public static void Save()
    {
        string text = Toml.FromModel(Instance);
        File.WriteAllText(CfgPath, text);
    }

    #endregion
}