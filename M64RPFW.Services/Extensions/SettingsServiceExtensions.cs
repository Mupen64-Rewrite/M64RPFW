namespace M64RPFW.Services.Extensions;

/// <summary>
///     An extension <see cref="class" /> which provides wrapper functions for <see cref="ILocalSettingsService" />
/// </summary>
public static class SettingsServiceExtensions
{
    /// <summary>
    ///     Tries getting a <see href="setting" /> as type <typeparamref name="T" />
    /// </summary>
    /// <param name="localSettingsService">The <see cref="ILocalSettingsService" /> to call <see cref="Get" /> on </param>
    /// <param name="key">A <see href="setting" />'s key</param>
    /// <param name="value">
    ///     The returned value as <paramref name="T" />, or <see langword="null" /> if <see langword="false" />
    ///     was returned
    /// </param>
    /// <typeparam name="T">The <see href="setting" />'s type</typeparam>
    /// <returns>
    ///     <see langword="true" /> if the setting could be retrieved, otherwise <see langword="false" />
    /// </returns>
    public static bool TryGet<T>(this ILocalSettingsService localSettingsService, string key, out T? value)
    {
        try
        {
            value = localSettingsService.Get<T>(key);
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }
}