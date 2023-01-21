namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that exposes an event for application closing
/// </summary>
public interface IApplicationClosingEventService
{
    /// <summary>
    /// The application closing event, which should fire when the application begins closing
    /// </summary>
    event Action OnApplicationClosing;
}