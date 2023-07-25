using SkiaSharp;

namespace M64RPFW.Services;

/// <summary>
///     Service exposing functionality of the Lua window.
/// </summary>
public interface ILuaWindowService
{

    /// <summary>
    ///     Prints to a visible console
    /// </summary>
    /// <param name="value">The value to be printed</param>
    public void Print(string value);
}