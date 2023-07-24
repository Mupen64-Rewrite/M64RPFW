

using M64RPFW.Services.Abstractions;

namespace M64RPFW.Services;

/// <summary>
/// Service that faciliates access to the main window.
/// </summary>
public interface IWindowAccessService
{
    WindowSize GetWindowSize();
    void SizeToFit(WindowSize size, bool sizeGlWindow = true);
    void UnlockWindowSize();
    
    WindowPoint PointerPosition { get; }
    MouseButtonMask PointerButtons { get; }

    public event EventHandler<SkiaRenderEventArgs> OnSkiaRender;
}