using SkiaSharp;

namespace M64RPFW.ViewModels.Scripting;

public partial class LuaEnvironment
{
    [LibFunction("skiasharp.get_canvas()")]
    private SKCanvas? SkiaSharp_GetCanvas()
    {
        return CurrentCanvas;
    }

    [LibFunction("skiasharp.create_offscreen_surface()")]
    private SKSurface? SkiaSharp_CreateOffscreenSurface(int width, int height)
    {
        return _skiaSurfaceManagerService?.CreateOffscreenBuffer(width, height);
    }
}