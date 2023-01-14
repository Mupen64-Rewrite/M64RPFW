namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides persistent settings functionality
/// </summary>
public interface ISettingsService
{
    /// <summary>
    ///     Gets a <see href="setting" /> as type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The <see href="setting" />'s type</typeparam>
    /// <param name="key">A <see href="setting" />'s key</param>
    /// <returns>
    ///     The <see href="setting" /> as type <typeparamref name="T" />, or throws an exception when the
    ///     <paramref name="key" /> is not found
    /// </returns>
    public T Get<T>(string key);

    /// <summary>
    ///     Sets a <see href="setting" /> as type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The <see href="setting" />'s type</typeparam>
    /// <param name="key">A <see href="setting" />'s key</param>
    /// <param name="saveAfter">Whether to call <see cref="Save" /> after applying this <see href="setting" /></param>
    /// <returns>
    ///     The <see href="setting" /> as type <typeparamref name="T" />, or throws an exception when the
    ///     <paramref name="key" /> is not found
    /// </returns>
    public void Set<T>(string key, T value, bool saveAfter = false);

    /// <summary>
    ///     Saves the current settings
    /// </summary>
    public void Save();
}