using Silk.NET.SDL;

namespace Mupen64PlusRR.Controls.Helpers;

public static class SilkGlobals
{
    private static Sdl? _sdl = null;
    public static Sdl SDL = _sdl ??= Sdl.GetApi();
}