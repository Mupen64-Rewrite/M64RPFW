using Avalonia;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.Views.Avalonia.Helpers;

public static class ServiceConversions
{
    public static WindowPoint ToWindowPoint(this Point point)
    {
        return new WindowPoint(point.X, point.Y);
    }
}