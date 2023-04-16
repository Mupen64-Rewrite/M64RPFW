namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides localization functionality
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    ///     Gets a resource value by the specified <paramref name="key" />
    /// </summary>
    /// <param name="key">A resource value's <paramref name="key" /></param>
    /// <param name="default">The default value</param>
    /// <returns>The resource value, or the default value if the <paramref name="key" /> is not present</returns>
    string GetStringOrDefault(string key, string @default = "");
}