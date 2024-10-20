namespace M64RPFW.Services.Abstractions;

public class SkiaRenderEventArgs : EventArgs
{
    public required ISkiaSurfaceManagerService SkiaSurfaceManager { get; init; }
}