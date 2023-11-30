namespace M64RPFW.Services.Abstractions;

public class SkiaRenderEventArgs : EventArgs
{
    public ISkiaSurfaceManagerService SkiaSurfaceManager { get; init; }
}