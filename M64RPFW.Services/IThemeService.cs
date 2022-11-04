namespace M64RPFW.Services
{
    /// <summary>
    /// The default <see langword="interface"/> for a service that provides <see href="View"/> themes
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the current theme <see cref="string"/>
        /// </summary>
        /// <returns>The current theme <see cref="string"/></returns>
        public string Get();

        /// <summary>
        /// Sets the current theme <see cref="string"/> to <paramref name="themeName"/>
        /// </summary>
        /// <param name="themeName">The new theme <see cref="string"/></param>
        public void Set(string themeName);
    }
}
