using SkiaSharp;

namespace M64RPFW.Services;

public interface ISkiaSurfaceManagerService
{
    public SKCanvas PrimaryCanvas { get; }
    public SKSurface CreateOffscreenBuffer(int width, int height);
}