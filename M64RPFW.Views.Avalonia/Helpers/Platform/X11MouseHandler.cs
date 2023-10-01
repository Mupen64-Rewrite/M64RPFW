using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Views;

namespace M64RPFW.Views.Avalonia.Helpers.Platform;

public class X11MouseHandler : INativeWindowMouseHandler
{
    private WeakReference<MainWindow> _mainWindow;
    private EventHandler<PointerEventArgs> _pointerEventHandler;
    private EventHandler<PointerPressedEventArgs> _pointerPressedEventHandler;
    private EventHandler<PointerReleasedEventArgs> _pointerReleasedEventHandler;

    public X11MouseHandler(MainWindow mainWindow)
    {
        _mainWindow = new WeakReference<MainWindow>(mainWindow);
        _pointerEventHandler = PointerEventHandler;
        _pointerPressedEventHandler = (sender, args) => PointerEventHandler(sender, args);
        _pointerReleasedEventHandler = (sender, args) => PointerEventHandler(sender, args);
        
        mainWindow.AddHandler(InputElement.PointerPressedEvent, _pointerPressedEventHandler, RoutingStrategies.Tunnel);
        mainWindow.AddHandler(InputElement.PointerReleasedEvent, _pointerReleasedEventHandler, RoutingStrategies.Tunnel);
        mainWindow.AddHandler(InputElement.PointerMovedEvent, _pointerEventHandler, RoutingStrategies.Tunnel);
    }

    private void PointerEventHandler(object? sender, PointerEventArgs e)
    {
        if (!_mainWindow.TryGetTarget(out var mainWindow))
            return;
        if (!mainWindow.GlControl.Bounds.Contains(e.GetPosition(null)))
            return;

        var pos = e.GetCurrentPoint(mainWindow.GlControl);
        Position = pos.Position;

        
        MouseButtonMask buttons = 0;
        var props = pos.Properties;
        buttons |= props.IsLeftButtonPressed ? MouseButtonMask.Primary : 0;
        buttons |= props.IsMiddleButtonPressed ? MouseButtonMask.Middle : 0;
        buttons |= props.IsRightButtonPressed ? MouseButtonMask.Secondary : 0;

        ButtonMask = buttons;
    }

    public Point Position { get; private set; } = new Point(0, 0);
    public MouseButtonMask ButtonMask { get; private set; } = 0;
}