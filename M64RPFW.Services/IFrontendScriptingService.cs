using SkiaSharp;

namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides frontend-specific scripting functionality
/// </summary>
public interface IFrontendScriptingService
{
    /// <summary>
    ///     Gets the current <see cref="IWindowAccessService" />
    /// </summary>
    public IWindowAccessService WindowAccessService { get; }

    /// <summary>
    ///     Prints to a visible console
    /// </summary>
    /// <param name="value">The value to be printed</param>
    public void Print(string value);
}