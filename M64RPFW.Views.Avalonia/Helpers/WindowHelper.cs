using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace M64RPFW.Views.Avalonia.Helpers;

internal class WindowHelper
{
    internal static Window GetWindow()
    {
        var lifetime = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return lifetime?.MainWindow!;
    }
}