using M64RPFW.Services.Abstractions;

namespace M64RPFW.Services;

/// <summary>
/// Service exposing rendering and input from the main window.
/// </summary>
public interface ILuaInterfaceService : IWindowAccessService
{
    public WindowPoint PointerPosition { get; }
    public MouseButtonMask PointerButtons { get; }
    public event EventHandler<SkiaRenderEventArgs>? OnSkiaRender;
}