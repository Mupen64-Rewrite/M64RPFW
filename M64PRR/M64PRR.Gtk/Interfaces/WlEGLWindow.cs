using System;
using System.Drawing;
using System.Runtime.InteropServices;
using WaylandSharp;

namespace M64PRR.Gtk.Interfaces;

/// <summary>
/// Wrapper class for wl_egl_window, which isn't a traditional Wayland type.
/// Practically the entire interface of <c>libwayland-egl.so</c>.
/// </summary>
public class WlEGLWindow : IDisposable
{
    private const string LibName = "libwayland-egl.so";
    
    // DLL-imported functions
    // ==========================================
    
    [DllImport(LibName)]
    private static extern IntPtr wl_egl_window_create(IntPtr surface, int width, int height);

    [DllImport(LibName)]
    private static extern void wl_egl_window_destroy(IntPtr window);

    [DllImport(LibName)]
    private static extern void wl_egl_window_get_attached_size(IntPtr window, out int width, out int height);
    
    [DllImport(LibName)]
    private static extern void wl_egl_window_resize(IntPtr window, int width, int height, int dx, int dy);
    
    // API
    // ==========================================
    
    private readonly IntPtr _pointer;
    public IntPtr RawPointer => _pointer;

    public WlEGLWindow(WlSurface surface, Size size)
    {
        _pointer = wl_egl_window_create(surface.RawPointer, size.Width, size.Height);
    }
    
    /// <summary>
    /// The EGL window's size.
    /// </summary>
    public Size Size
    {
        get
        {
            wl_egl_window_get_attached_size(window: _pointer, out var width, out var height);
            return new Size(width, height);
        }
        set
        {
            wl_egl_window_resize(window: _pointer, value.Width, value.Height, 0, 0);
        }
    }

    private void ReleaseUnmanagedResources()
    {
        wl_egl_window_destroy(_pointer);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~WlEGLWindow()
    {
        ReleaseUnmanagedResources();
    }
}