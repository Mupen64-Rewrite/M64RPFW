using Avalonia;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Helpers.Platform;
using M64RPFW.Views.Avalonia.Views;

namespace M64RPFW.Views.Avalonia.Helpers;

internal interface INativeWindowMouseHandler
{
    static INativeWindowMouseHandler? GetMouseLocator(MainWindow window)
    {
        var desc = window.TryGetPlatformHandle()?.HandleDescriptor;
        switch (desc)
        {
            case "HWND":
                return new WindowsMouseHandler(window);
            case "XID":
                return new X11MouseHandler(window);
            default:
                return null;
        }
    }

    Point Position { get; }
    MouseButtonMask ButtonMask { get; }
}