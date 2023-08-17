

using M64RPFW.Services.Abstractions;

namespace M64RPFW.Services;

/// <summary>
/// Service exposing direct functions of the main window.
/// </summary>
public interface IWindowAccessService
{
    WindowSize GetWindowSize();
    void SizeToFit(WindowSize size);
    void UnlockWindowSize();
    
    nint WindowHandle { get; }
    WindowSystemID WindowSystemID { get; }
}