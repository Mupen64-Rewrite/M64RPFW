using M64RPFW.Services.Abstractions;

namespace M64RPFW.Services;

/// <summary>
/// A service for capturing output.
/// </summary>
public unsafe interface IFrameCaptureService
{
    public WindowSize GetWindowSize();

    /// <summary>
    /// Renders the current screen state to a buffer in RGB0 format.
    /// </summary>
    /// <param name="buffer">A buffer to contain the data.</param>
    /// <param name="linesize">The size of each line, in bytes.</param>
    public void CaptureTo(void* buffer, uint linesize);

    public event Action? OnRender;
}