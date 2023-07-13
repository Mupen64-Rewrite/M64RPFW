namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that invokes the application dispatcher
/// </summary>
public interface IDispatcherService
{
	/// <summary>
	///     Queues an <see cref="Action" /> on the dispatcher
	/// </summary>
	/// <param name="action">The action to be executed</param>
	void Execute(Action action);
	
	/// <summary>
	///     Queues an <see cref="Action" /> on the dispatcher and waits for it to complete.
	/// </summary>
	/// <param name="action">The action to be executed</param>
	void ExecuteAndWait(Action action);
}