using SkiaSharp;

namespace M64RPFW.Services.Abstractions;

public class SkiaRenderEventArgs : EventArgs
{
    public SKCanvas Canvas { get; init; }
}