using System;
using System.Runtime.InteropServices;

namespace M64RPFW.Views.Avalonia.Native;

public unsafe class LibX11
{
    public enum Window : ulong
    {
        None = 0
    }

    public enum Colormap : ulong { }

    public enum Pixmap : ulong { }

    public enum Cursor : ulong { }

    public enum EventMask : long
    {
        /// <summary>
        /// These are the input event masks from X.h
        /// </summary>
        NoEventMask = 0L,
        KeyPressMask = (1L << 0),
        KeyReleaseMask = (1L << 1),
        ButtonPressMask = (1L << 2),
        ButtonReleaseMask = (1L << 3),
        EnterWindowMask = (1L << 4),
        LeaveWindowMask = (1L << 5),
        PointerMotionMask = (1L << 6),
        PointerMotionHintMask = (1L << 7),
        Button1MotionMask = (1L << 8),
        Button2MotionMask = (1L << 9),
        Button3MotionMask = (1L << 10),
        Button4MotionMask = (1L << 11),
        Button5MotionMask = (1L << 12),
        ButtonMotionMask = (1L << 13),
        KeymapStateMask = (1L << 14),
        ExposureMask = (1L << 15),
        VisibilityChangeMask = (1L << 16),
        StructureNotifyMask = (1L << 17),
        ResizeRedirectMask = (1L << 18),
        SubstructureNotifyMask = (1L << 19),
        SubstructureRedirectMask = (1L << 20),
        FocusChangeMask = (1L << 21),
        PropertyChangeMask = (1L << 22),
        ColormapChangeMask = (1L << 23),
        OwnerGrabButtonMask = (1L << 24),
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XImage
    {
        public int width, height;
        public int xoffset;
        public int format;
        public IntPtr data;
        public int byte_order;
        public int bitmap_unit;
        public int bitmap_bit_order;
        public int bitmap_pad;
        public int depth;
        public int bytes_per_line;
        public int bits_per_pixel;
        public ulong red_mask;
        public ulong green_mask;
        public ulong blue_mask;
        public IntPtr obdata;

        private struct funcs
        {
            IntPtr create_image;
            IntPtr destroy_image;
            IntPtr get_pixel;
            IntPtr put_pixel;
            IntPtr sub_image;
            IntPtr add_pixel;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XWindowAttributes
    {
        public int x, y;
        public uint width, height;
        public int border_width;
        public int depth;
        public IntPtr visual;
        public Window root;
        public int @class;
        public int bit_gravity;
        public int win_gravity;
        public int backing_store;
        public ulong backing_planes;
        public ulong backing_pixel;
        public bool save_under;
        public Colormap colormap;
        public bool map_installed;
        public int map_state;
        public long all_event_masks;
        public long your_event_masks;
        public long do_not_propagate_mask;
        public bool override_redirect;
        public IntPtr screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XSetWindowAttributes
    {
        public Pixmap background_pixmap; /* background or None or ParentRelative */
        public ulong background_pixel; /* background pixel */
        public Pixmap border_pixmap; /* border of the window */
        public ulong border_pixel; /* border pixel value */
        public int bit_gravity; /* one of bit gravity values */
        public int win_gravity; /* one of the window gravity values */
        public int backing_store; /* NotUseful, WhenMapped, Always */
        public ulong backing_planes; /* planes to be preseved if possible */
        public ulong backing_pixel; /* value to use in restoring planes */
        public bool save_under; /* should bits under be saved? (popups) */
        public EventMask event_mask; /* set of events that should be saved */
        public EventMask do_not_propagate_mask; /* set of events that should not propagate */
        public bool override_redirect; /* boolean value for override-redirect */
        public Colormap colormap; /* color map to be associated with window */
        public Cursor cursor;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XWindowChanges
    {
        public int x, y;
        public int width, height;
        public int border_width;
        public Window sibling;
        public int stack_mode;
    }

    public struct Display { }

    [Flags]
    public enum ValueMask : ulong
    {
        BackPixmap = 1L << 0,
        BackPixel = 1L << 1,
        BorderPixmap = 1L << 2,
        BorderPixel = 1L << 3,
        BitGravity = 1L << 4,
        WinGravity = 1L << 5,
        BackingStore = 1L << 6,
        BackingPlanes = 1L << 7,
        BackingPixel = 1L << 8,
        OverrideRedirect = 1L << 9,
        SaveUnder = 1L << 10,
        EventMask = 1L << 11,
        DontPropagate = 1L << 12,
        Colormap = 1L << 13,
        Cursor = 1L << 14,
    }

    [DllImport("libX11.so.6")]
    public static extern Display* XOpenDisplay([MarshalAs(UnmanagedType.LPStr)] string? name);

    [DllImport("libX11.so.6")]
    public static extern int XCloseDisplay(Display* disp);
    
    [DllImport("libX11.so.6")]
    public static extern int XChangeWindowAttributes(Display* disp, Window w, ValueMask valuemask, in XSetWindowAttributes attributes);
}
/*

*/