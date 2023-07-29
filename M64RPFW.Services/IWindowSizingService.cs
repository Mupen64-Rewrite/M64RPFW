

using M64RPFW.Services.Abstractions;

namespace M64RPFW.Services;

/// <summary>
/// Service exposing direct functions of the main window.
/// </summary>
public interface IWindowSizingService
{
    WindowSize GetWindowSize();
    void SizeToFit(WindowSize size);
    void UnlockWindowSize();
}