using System.Collections.ObjectModel;
using System.Text;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;
using Tomlyn;
using Tomlyn.Model;

namespace M64RPFW.Models.Settings;

/// <summary>
/// Settings handler for the M64RPFW frontend.
/// </summary>
public class RPFWSettings : ITomlMetadataProvider
{
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    public class ViewSection : ITomlMetadataProvider
    {
        // storage for comments and whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

        public string Culture { get; set; } = "en-US";
        public string Theme { get; set; } = "Light";
        public ObservableCollection<string> RecentRoms { get; set; } = new();
        public ObservableCollection<string> RecentLuaScripts { get; set; } = new();
        public bool IsStatusBarVisible { get; set; } = true;
    }

    public ViewSection View { get; } = new();
    
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
    
    public class HotkeysSection : ITomlMetadataProvider
    {
        // storage for comments and whitespace
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        
        // TODO: match back to default Mupen64 keymap
        
        public string OpenRom { get; set; } = "Ctrl+O";
        public string CloseRom { get; set; } = "Ctrl+W";
        public string ResetRom { get; set; } = "Ctrl+R";

        public string PauseOrResume { get; set; } = "P";
        public string FrameAdvance { get; set; } = "F";
        
        public string FastForward { get; set; } = "Z";

        public string LoadFromFile { get; set; } = "Ctrl+Shift+O";
        public string SaveToFile { get; set; } = "Ctrl+Shift+S";
        public string LoadCurrentSlot { get; set; } = "L";
        public string SaveCurrentSlot { get; set; } = "I";

        public string StartMovie { get; set; } = "Ctrl+Alt+O";
        public string StartRecording { get; set; } = "Ctrl+Alt+L";
        public string StopMovie { get; set; } = "Ctrl+Alt+W";
        public string RestartMovie { get; set; } = "Ctrl+Alt+R";
        public string DisableWrites { get; set; } = "Alt+Shift+R";
    }

    public HotkeysSection Hotkeys { get; } = new();


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