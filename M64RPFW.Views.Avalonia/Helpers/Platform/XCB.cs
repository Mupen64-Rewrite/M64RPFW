using System;
using System.Runtime.InteropServices;
using XID = System.UInt32;
namespace M64RPFW.Views.Avalonia.Helpers.Platform;

internal static unsafe partial class XCB
{
    [DllImport(LibX11_XCB, EntryPoint = "XGetXCBConnection")]
    public static extern Connection* Xlib_XGetXCBConnection(void* dpy);

    /// <summary>
    /// Allocates an XID for a new object. Typically used just prior to various object creation functions, such as xcb_create_window.
    /// </summary>
    /// <param name="c">The connection.</param>
    /// <returns>A newly allocated XID, or <see cref="uint.MaxValue"/> on failure.</returns>
    [DllImport(LibXCB, EntryPoint = "xcb_generate_id")]
    public static extern uint GenerateID(Connection* c);

    [DllImport(LibXCB_XFixes, EntryPoint = "xcb_xfixes_create_region")]
    public static extern VoidCookie XFixesCreateRegion(Connection* c, uint region, uint rectanglesLen, Rectangle* rectangles);

    [DllImport(LibXCB_XFixes, EntryPoint = "xcb_xfixes_destroy_region")]
    public static extern VoidCookie XFixesDestroyRegion(Connection* c, uint region);

    [DllImport(LibXCB_XFixes, EntryPoint = "xcb_xfixes_set_window_shape_region_checked")]
    public static extern VoidCookie XFixesSetWindowShapeRegionChecked(Connection* c, uint dest, byte destKind, short xOffset, short yOffset, uint region);

    public static VoidCookie XFixesCreateRegion(Connection* c, uint region, Span<Rectangle> rectangles)
    {
        fixed (Rectangle* pRectangles = rectangles)
        {
            return XFixesCreateRegion(c, region, (uint) rectangles.Length, pRectangles);
        }
    }
    
    public static VoidCookie XFixesCreateRegion(Connection* c, uint region, in Rectangle rectangle)
    {
        fixed (Rectangle* pRectangle = &rectangle)
        {
            return XFixesCreateRegion(c, region, 1, pRectangle);
        }
    }

    public static VoidCookie XFixesSetWindowShapeRegionChecked(Connection* c, uint dest, ShapeSK destKind, short xOffset, short yOffset, uint region)
    {
        return XFixesSetWindowShapeRegionChecked(c, dest, (byte) destKind, xOffset, yOffset, region);
    }
    
}