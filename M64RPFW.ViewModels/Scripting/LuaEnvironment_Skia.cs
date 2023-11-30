using SkiaSharp;

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    [LibFunction("skiasharp.get_canvas()")]
    private SKCanvas? SkiaGetCanvas()
    {
        return _skCanvas;
    }
}