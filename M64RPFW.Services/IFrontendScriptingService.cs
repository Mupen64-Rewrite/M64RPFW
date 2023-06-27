namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides frontend-specific scripting functionality
/// </summary>
public interface IFrontendScriptingService
{
    /// <summary>
    ///     Prints to a visible console
    /// </summary>
    /// <param name="value">The value to be printed</param>
    public void Print(string value);

    /// <summary>
    ///     Gets the size of the main window
    /// </summary>
    /// <returns>The size of the main window</returns>
    public (int Width, int Height) GetWindowSize();

    /// <summary>
    ///     Sets the size of the main window
    /// </summary>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    public void SetWindowSize(int width, int height);
}