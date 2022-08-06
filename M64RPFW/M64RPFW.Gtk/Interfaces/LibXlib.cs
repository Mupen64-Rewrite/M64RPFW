using System;
using System.Runtime.InteropServices;
using X11;

namespace M64RPFW.Gtk.Interfaces;

/// <summary>
/// Xlib functions not wrapped by X11.NET.
/// </summary>
public class LibXlib
{
    private const string LibName = "libX11.so.6";

    [DllImport(LibName)]
    public static extern int XChangeWindowAttributes(IntPtr display, Window window, ulong valuemask, ref XSetWindowAttributes attributes);

    [DllImport(LibName)]
    public static extern int XSetTransientForHint(IntPtr display, Window window, Window transientTo);
    
}