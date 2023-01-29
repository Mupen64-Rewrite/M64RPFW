using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using M64RPFW.Services;

namespace M64RPFW;

internal sealed class LocalSettings : ILocalSettingsService
{
    public static LocalSettings Default = new()
    {
        _settings = new Dictionary<string, object>
        {
            { "RecentRomPaths", new List<string>() },
            { "RomExtensions", new[] { "n64", "z64", "rom", "eu", "usa", "jp" } },
            { "CoreLibraryPath", "m64p/mupen64plus.dll" },
            { "VideoPluginPath", "m64p/mupen64plus-video-rice.dll" },
            { "AudioPluginPath", "m64p/mupen64plus-audio-sdl.dll" },
            { "InputPluginPath", "m64p/mupen64plus-input-sdl.dll" },
            { "RspPluginPath", "m64p/mupen64plus-rsp-hle.dll" },
            { "CoreType", 2 },
            { "ScreenWidth", 800 },
            { "ScreenHeight", 600 },
            { "IsStatusbarVisible", true },
            { "Culture", "en-US" },
            { "Theme", "Light" }
        }
    };

    private Dictionary<string, object> _settings = new();

    private LocalSettings()
    {
    }

    public event EventHandler<string>? OnSettingChanged;

    public T Get<T>(string key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        if (!_settings.TryGetValue(key, out var value))
            throw new ArgumentException($"Attempted to retrieve nonexistent key \"{key}\"");

        if (value is T valueAsT)
            return valueAsT;

        throw new ArgumentException($"Requested type was {typeof(T)}, but got {value.GetType()}");
    }

    public void Set<T>(string key, T value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (value == null) throw new ArgumentNullException(nameof(value));

        _settings[key] = value;
        OnSettingChanged?.Invoke(this, key);
    }

    public void InvokeOnSettingChangedForAllKeys()
    {
        foreach (var keyValuePair in _settings) OnSettingChanged?.Invoke(this, keyValuePair.Key);
    }

    public static LocalSettings FromJson(string json)
    {
        var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        ArgumentNullException.ThrowIfNull(settings);

        // all values on `settings` are JSONElements because their type is unknown
        // we need to cast and finish those up before returning

        // prune all keys which dont exist anymore
        foreach (var pair in settings)
            if (!Default._settings.ContainsKey(pair.Key))
            {
                settings.Remove(pair.Key);
                Debug.WriteLine($"Pruned removed key \"{pair.Key}\"");
            }


        // backwards-compatibility:
        // if the internal default dictionary has pairs (settings) which dont exist in the (possibly older) settings file,
        // create them and give them the default value
        foreach (var defaultPair in Default._settings)
            if (!settings.TryGetValue(defaultPair.Key, out var value))
            {
                settings[defaultPair.Key] = defaultPair.Value;
                Debug.WriteLine($"Merged new key \"{defaultPair.Key}\"");
            }

        // cast those bitches
        foreach (var pair in settings)
        {
            var intendedType = Default._settings[pair.Key].GetType();
            dynamic newValue = ((JsonElement)pair.Value).Deserialize(intendedType);
            Convert.ChangeType(newValue, intendedType);
            settings[pair.Key] = newValue;
        }

        return new LocalSettings
        {
            _settings = settings
        };
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(_settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}