using System;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

/// <summary>
/// Exception thrown by SDL when things go bad.
/// </summary>
public class SDLException : SystemException
{
    public SDLException() : base(SDLHelpers.sdl.GetErrorS()) { }
    public SDLException(Exception inner) : base(SDLHelpers.sdl.GetErrorS(), inner) { }
}