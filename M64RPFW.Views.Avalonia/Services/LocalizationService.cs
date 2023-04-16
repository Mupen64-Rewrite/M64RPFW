using M64RPFW.Services;

namespace M64RPFW.Views.Avalonia.Services;

public class LocalizationService : ILocalizationService
{
    public string GetStringOrDefault(string key, string @default = "") => Resources.Resources.ResourceManager.GetString(key) ?? @default;
}