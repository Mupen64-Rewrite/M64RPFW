namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that represents the application dispatcher
/// </summary>
public interface IDispatcherService
{
    /// <summary>
    ///     Queues an <see cref="Action" /> on the dispatcher
    /// </summary>
    /// <param name="action">The action to be invoked by the dispatcher</param>
    void Execute(Action action);
}