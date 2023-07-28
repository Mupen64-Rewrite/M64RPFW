using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.OpenGL;

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
            // .With(new Win32PlatformOptions
            // {
            //     // this seems to be necessary
            //     RenderingMode = new[] { Win32RenderingMode.Wgl },
            //     WglProfiles = new List<GlVersion>(new[] {new GlVersion(GlProfileType.OpenGL, 3, 3)})
            // })
            // .With(new X11PlatformOptions
            // {
            //     // GLX is broken for some reason, we don't know.
            //     RenderingMode = new[] { X11RenderingMode.Egl },
            //     GlProfiles = new List<GlVersion>(new[] {new GlVersion(GlProfileType.OpenGL, 3, 3)})
            // })
            ;
        return builder;
    }
}