﻿using System;
using Avalonia;

namespace M64RPFW.Views.Avalonia;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        // NOTE: may need to force X11 on Linux when Wayland support is added, as GNOME
        // does not support the xdg-decoration protocol for SSD.
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .With(new Win32PlatformOptions
            {
                // add options here as needed
                UseWgl = true
            })
            .With(new X11PlatformOptions
            {
                // this seems to be necessary
                UseEGL = true
            });
        // Native file dialogs on Linux broke in Avalonia 10, so...
        // if (OperatingSystem.IsLinux())
        //     builder = builder.UseManagedSystemDialogs();
        return builder;
    }
}