using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;

namespace M64RPFW.Models.Settings;
using LogSources = Mupen64Plus.LogSources;
using MessageLevel = Mupen64PlusTypes.MessageLevel;

/// <summary>
/// Settings handler for the M64RPFW frontend.
/// </summary>
public static class RPFWSettings
{
    private static string? _jsonPath;
    private static string JsonPath => _jsonPath ??= Path.Join(MupenSettings.GetUserConfigPath(), "m64rpfw.json");

    private static JsonObject? _settingsRoot;
    
    /// <summary>
    /// Returns the root JSON object of the settings.
    /// </summary>
    public static JsonObject Root
    {
        get
        {
            if (_settingsRoot == null)
                Load();
            return _settingsRoot!;
        }
    }

    public static void Load()
    {
        using var file = File.OpenRead(JsonPath);
        try
        {
            _settingsRoot = JsonSerializer.Deserialize<JsonObject>(file);
        }
        catch (JsonException)
        {
            _settingsRoot = null;
        }
        // if we loaded settings we're done
        if (_settingsRoot != null)
            return;
        
        Mupen64Plus.Log(LogSources.Config, MessageLevel.Warning, "Failed to load M64RPFW settings, resetting to defaults...");
        // TODO: Do we need to prompt the user here? How do we do that without breaking MVVM?
        _settingsRoot = InitDefaultSettings();
    }

    public static void SetAllToDefault()
    {
        _settingsRoot = InitDefaultSettings();
    }

    public static void SetUnknownsToDefault()
    {
        _settingsRoot = InitDefaultSettings(_settingsRoot);
    }

    public static void Save()
    {
        using var file = File.OpenWrite(JsonPath);
        JsonSerializer.Serialize(file, _settingsRoot);
    }

    private static JsonObject InitDefaultSettings(JsonObject? prev = null)
    {
        var obj = prev ?? new JsonObject();
        // setup default settings here
        return obj;
    }
}