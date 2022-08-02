using System;
using System.Runtime.InteropServices;
using WaylandSharp;

namespace M64PRR.Gtk.Interfaces;

public class LibGdk
{
    private const string LibName = "libgdk-3.so";

    // DLL-imported functions
    // ==========================================
    [DllImport(LibName)]
    private static extern IntPtr gdk_x11_display_get_type();

    [DllImport(LibName)]
    private static extern ulong gdk_x11_window_get_xid(IntPtr handle);

    [DllImport(LibName)]
    private static extern IntPtr gdk_wayland_display_get_type();

    [DllImport(LibName)]
    private static extern IntPtr gdk_wayland_display_get_wl_display(IntPtr display);
    
    [DllImport(LibName)]
    private static extern IntPtr gdk_wayland_display_get_wl_compositor(IntPtr display);

    [DllImport(LibName)]
    private static extern IntPtr gdk_wayland_window_get_wl_surface(IntPtr window);

    // API
    // ==========================================

    public static bool Gdk_IsX11Display(Gdk.Display display)
    {
        GLib.GType type = new GLib.GType(gdk_x11_display_get_type());
        return LibGLib.GType_CheckInstanceType(display, type);
    }

    public static bool Gdk_IsWaylandDisplay(Gdk.Display display)
    {
        GLib.GType type = new GLib.GType(gdk_wayland_display_get_type());
        return LibGLib.GType_CheckInstanceType(display, type);
    }

    public static X11.Window GdkX11Window_GetXID(Gdk.Window window)
    {
        return (X11.Window) gdk_x11_window_get_xid(window.Handle);
    }

    public static unsafe WlDisplay GdkWaylandDisplay_GetWlDisplay(Gdk.Display display)
    {
        return new WlDisplay((_WlProxy*) gdk_wayland_display_get_wl_display(display.Handle).ToPointer());
    }

    public static unsafe WlCompositor GdkWaylandDisplay_GetWlCompositor(Gdk.Display display)
    {
        return new WlCompositor((_WlProxy*) gdk_wayland_display_get_wl_compositor(display.Handle).ToPointer());
    }

    public static unsafe WlSurface GdkWaylandWindow_GetWlSurface(Gdk.Window window)
    {
        return new WlSurface((_WlProxy*) gdk_wayland_window_get_wl_surface(window.Handle).ToPointer());
    }
}