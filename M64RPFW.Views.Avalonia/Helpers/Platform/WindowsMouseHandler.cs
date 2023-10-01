using System;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Controls.Helpers;
using M64RPFW.Views.Avalonia.Views;
using Silk.NET.SDL;
using Point = Avalonia.Point;

namespace M64RPFW.Views.Avalonia.Helpers.Platform;

public class WindowsMouseHandler : INativeWindowMouseHandler
{
    private WeakReference<MainWindow> _window;

    public WindowsMouseHandler(MainWindow window)
    {
        _window = new WeakReference<MainWindow>(window);
    }


    public unsafe Point Position
    {
        get
        {
            if (!_window.TryGetTarget(out var window))
                throw new ObjectDisposedException("Parent window does not exist!");
            var sdlWin = window.GlControl._sdlWin;

            int x = 0, y = 0;
            SDLHelpers.sdl.GetMouseState(ref x, ref y);
            return new Point(x, y);
        }
    }
    public unsafe MouseButtonMask ButtonMask
    {
        get
        {
            if (!_window.TryGetTarget(out var window))
                throw new ObjectDisposedException("Parent window does not exist!");
            var sdlWin = window.GlControl._sdlWin;

            int x = 0, y = 0;
            var status = SDLHelpers.sdl.GetMouseState(ref x, ref y);

            return (MouseButtonMask) status;
        }
    }
}