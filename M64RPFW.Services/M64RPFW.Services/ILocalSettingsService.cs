namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides persistent settings functionality
/// </summary>
public interface ILocalSettingsService
{
    /// <summary>
    ///     Raised when a setting's value changes
    /// </summary>
    event EventHandler<string> OnSettingChanged;

    /// <summary>
    ///     Retrieves a setting of type <typeparamref name="T" />
    /// </summary>
    /// <param name="key">The setting's key</param>
    /// <typeparam name="T">The setting value's type</typeparam>
    /// <returns>If the operation succeeds, the setting value, otherwise an exception will be thrown</returns>
    T Get<T>(string key);

    /// <summary>
    ///     Sets a setting of type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The setting value's type</typeparam>
    /// <param name="key">The setting's key</param>
    /// <returns>
    ///     The setting as type <typeparamref name="T" />, or throws an exception when the
    ///     <paramref name="key" /> is not found
    /// </returns>
    void Set<T>(string key, T value);
}