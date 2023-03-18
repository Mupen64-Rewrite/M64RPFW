using Silk.NET.SDL;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public static class SilkGlobals
{
    private static Sdl? _sdl = null;
    public static Sdl SDL = _sdl ??= Sdl.GetApi();
}