using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using FFmpeg.AutoGen;

namespace M64RPFW.Views.Avalonia;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        SetupConsole();
        InitFFmpeg();
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
    
    [DllImport("kernel32.dll", EntryPoint = "AttachConsole", SetLastError = true)]
    [return : MarshalAs(UnmanagedType.Bool)]
    private static extern bool Win32_AttachConsole(uint dwProcessId);

    private const uint Win32_AttachParentProcess = uint.MaxValue;

    private static void SetupConsole()
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (Win32_AttachConsole(Win32_AttachParentProcess))
            return;
        
        int err = Marshal.GetLastPInvokeError();
        if (err != 6)
            throw new Win32Exception(err);
    }

    private static void InitFFmpeg()
    {
        if (OperatingSystem.IsWindows())
            return;
        if (OperatingSystem.IsLinux())
        {
            if (File.Exists("/lib/libavcodec.so"))
                ffmpeg.RootPath = "/lib";
            else if (File.Exists("/usr/lib/libavcodec.so"))
                ffmpeg.RootPath = "/usr/lib";
            else if (File.Exists("/lib64/libavcodec.so"))
                ffmpeg.RootPath = "/lib64";
            else if (File.Exists("/usr/lib64/libavcodec.so"))
                ffmpeg.RootPath = "/usr/lib64";
            return;
        }
        if (OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("Can't autodetect FFmpeg install!");
        }
    }
}