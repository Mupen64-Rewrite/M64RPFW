namespace M64RPFW.Services.Abstractions;

public readonly record struct WindowSize(int Width, int Height)
{
    public override string ToString()
    {
        return $"{Width}x{Height}";
    }
}

public readonly record struct WindowPoint(int X, int Y)
{
    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}

[Flags]
public enum MouseButtonMask
{
    Primary = (1 << 0),
    Middle = (1 << 1),
    Secondary = (1 << 2)
}