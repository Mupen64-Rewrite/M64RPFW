using SkiaSharp;

namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides frontend-specific scripting functionality
/// </summary>
public interface IFrontendScriptingService
{
    /// <summary>
    /// Gets the current <see cref="IWindowSizingService"/>
    /// </summary>
    public IWindowSizingService WindowSizingService { get; }

    /// <summary>
    /// An event, which is invoked when the frontend is ready to draw
    /// <para/>
    /// A Skia drawing context of type <see cref="SKCanvas"/> is provided to draw with, but ownership must be relinquished
    /// after the event handler exits
    /// </summary>
    public event Action<SKCanvas> OnUpdateScreen;

    /// <summary>
    ///     Prints to a visible console
    /// </summary>
    /// <param name="value">The value to be printed</param>
    public void Print(string value);
}