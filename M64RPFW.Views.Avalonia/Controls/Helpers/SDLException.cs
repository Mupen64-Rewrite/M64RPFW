using System;
using static M64RPFW.Views.Avalonia.Controls.Helpers.SDLHelpers;

namespace M64RPFW.Views.Avalonia.Controls.Helpers;

/// <summary>
/// Exception thrown by SDL when things go bad.
/// </summary>
public class SDLException : SystemException
{
    public SDLException() : base(sdl.GetErrorS()) { }
    public SDLException(Exception inner) : base(sdl.GetErrorS(), inner) { }
}