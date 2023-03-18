namespace M64RPFW.Services.Abstractions;

/// <summary>
///     The default <see langword="interface" /> for a service that provides repeated method invocation functionality
/// </summary>
public interface ITimer
{
	/// <summary>
	///     Whether the timer is resumed
	/// </summary>
	bool IsResumed { get; set; }
}