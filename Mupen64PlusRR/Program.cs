﻿using Avalonia;
using System;
using Avalonia.Dialogs;

namespace Mupen64PlusRR;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
        // Native file dialogs on Linux broke, so...
        if (OperatingSystem.IsLinux())
            builder = builder.UseManagedSystemDialogs();
        return builder;
    }
}