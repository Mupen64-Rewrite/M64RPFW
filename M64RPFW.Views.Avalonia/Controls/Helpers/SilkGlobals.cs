using Silk.NET.SDL;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

public static class SilkGlobals
{
    private static readonly Sdl? _sdl;
    public static Sdl SDL = _sdl ??= Sdl.GetApi();
}