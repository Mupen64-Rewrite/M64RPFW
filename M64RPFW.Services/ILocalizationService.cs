namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides localization functionality
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    ///     Gets a <see href="resource value" /> by the specified <paramref name="key" />
    /// </summary>
    /// <param name="key">A <see href="resource value" />'s <paramref name="key" /></param>
    /// <returns>The <see href="resource value" />, or a placeholder value if the <paramref name="key" /> is not present</returns>
    public string GetString(string key);

    /// <summary>
    ///     Sets the <see href="Locale" /> to the specified <paramref name="localeKey" />
    /// </summary>
    /// <param name="localeKey">
    ///     The locale key, formatted according to
    ///     <see href="https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo#culture-names-and-identifiers" />
    /// </param>
    public void SetLocale(string localeKey);
}