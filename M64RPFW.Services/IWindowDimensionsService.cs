namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides window dimensions
/// </summary>
public interface IWindowDimensionsService
{
    /// <summary>
    /// An event which is invoked when window dimensions change
    /// </summary>
    event Action<(double Width, double Height)> DimensionsChanged;

    /// <summary>
    /// The current window width
    /// </summary>
    double Width { get; set; }
    
    /// <summary>
    /// The current window height
    /// </summary>
    double Height { get; set; }
    
    /// <summary>
    /// The height of the app menu
    /// </summary>
    double MenuHeight { get; }
    
    /// <summary>
    /// Whether the window is manually resizable
    /// </summary>
    bool IsResizable { set; }
}