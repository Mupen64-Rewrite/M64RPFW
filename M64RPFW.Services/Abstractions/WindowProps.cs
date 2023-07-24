namespace M64RPFW.Services.Abstractions;

public record struct WindowSize(double Width, double Height);

public record struct WindowPoint(double X, double Y);

[Flags]
public enum MouseButtonMask
{
    Primary = (1 << 0),
    Middle = (1 << 1),
    Secondary = (1 << 2)
}