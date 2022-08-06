using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gdk;
using WaylandSharp;

namespace M64RPFW.Gtk.Interfaces;

public class LibGdk
{
    private const string LibName = "libgdk-3.so";

    // DLL-imported functions
    // ==========================================
    [DllImport(LibName)]
    private static extern IntPtr gdk_x11_display_get_type();

    [DllImport(LibName)]
    private static extern IntPtr gdk_x11_display_get_xdisplay(IntPtr handle);

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

    // Wayland: tracking objects
    // ==========================================
    private class IdentityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    private static Dictionary<Window, WlSurface> _windowDict;

    static LibGdk()
    {
        _windowDict = new Dictionary<Window, WlSurface>(new IdentityComparer<Window>());
    }

    // API
    // ==========================================

    /// <summary>
    /// Equivalent to the C macro <c>GDK_IS_X11_DISPLAY(display)</c>.
    /// </summary>
    /// <param name="display">A <see cref="Gdk.Display"/></param>
    /// <returns><c>true</c> if the underlying <c>GdkDisplay</c> is a <c>GdkX11Display</c></returns>
    public static bool Gdk_IsX11Display(Gdk.Display display)
    {
        GLib.GType type = new GLib.GType(gdk_x11_display_get_type());
        return LibGObject.GType_CheckInstanceType(display, type);
    }

    /// <summary>
    /// Equivalent to the C macro <c>GDK_IS_WAYLAND_DISPLAY(display)</c>.
    /// </summary>
    /// <param name="display">A <see cref="Gdk.Display"/></param>
    /// <returns><c>true</c> if the underlying <c>GdkDisplay</c> is a <c>GdkWaylandDisplay</c></returns>
    public static bool Gdk_IsWaylandDisplay(Gdk.Display display)
    {
        GLib.GType type = new GLib.GType(gdk_wayland_display_get_type());
        return LibGObject.GType_CheckInstanceType(display, type);
    }

    /// <summary>
    /// Returns the X11 display object corresponding to a <see cref="Gdk.Display"/>
    /// </summary>
    /// <param name="display">A <see cref="Gdk.Display"/></param>
    /// <returns>An opaque pointer to the display object (same as C XDisplay*)</returns>
    public static IntPtr GdkX11Display_GetXDisplay(Gdk.Display display)
    {
        return gdk_x11_display_get_xdisplay(display.Handle);
    }

    /// <summary>
    /// Returns the <see cref="X11.Window"/> corresponding to a <see cref="Gdk.Window"/>.
    /// </summary>
    /// <param name="window">a <see cref="Gdk.Window"/></param>
    /// <returns>A <see cref="X11.Window"/></returns>
    public static X11.Window GdkX11Window_GetXID(Gdk.Window window)
    {
        return (X11.Window) gdk_x11_window_get_xid(window.Handle);
    }

    /// <summary>
    /// Returns the <see cref="WlDisplay"/> corresponding to a <see cref="Gdk.Display"/>.
    /// </summary>
    /// <param name="display">A <see cref="Gdk.Display"/></param>
    /// <returns>A <see cref="WlDisplay"/></returns>
    public static unsafe WlDisplay GdkWaylandDisplay_GetWlDisplay(Gdk.Display display)
    {
        var res = new WlDisplay((_WlProxy*) gdk_wayland_display_get_wl_display(display.Handle).ToPointer());
        return res;
    }

    /// <summary>
    /// Returns the <see cref="X11.Window"/> corresponding to a <see cref="Gdk.Window"/>.
    /// </summary>
    /// <param name="window">a <see cref="Gdk.Window"/></param>
    /// <returns>A <see cref="X11.Window"/></returns>
    public static unsafe WlSurface GdkWaylandWindow_GetWlSurface(Gdk.Window window)
    {
        // WaylandSharp keeps track of objects it's registered; the same object
        // can't be registered twice. This means that we need to use a custom
        // dictionary (keyed by the objects' identity) to remember which windows
        // used which surfaces.
        if (_windowDict.TryGetValue(window, out var surface))
            return surface;
        
        var res = new WlSurface((_WlProxy*) gdk_wayland_window_get_wl_surface(window.Handle).ToPointer());
        _windowDict.Add(window, res);
        return res;
    }
}