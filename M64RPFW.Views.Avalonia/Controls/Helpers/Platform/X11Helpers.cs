using M64RPFW.Views.Avalonia.Helpers.Platform;

namespace M64RPFW.Views.Avalonia.Controls.Helpers.Platform;

internal static unsafe class X11Helpers
{
    /// <summary>
    /// Enables mouse passthrough for a window.
    /// </summary>
    /// <param name="dpy">The Xlib <c>Display*</c>.</param>
    /// <param name="win">The XID of the window. </param>
    /// <remarks>
    /// The code here is taken <a href="https://github.com/qt/qtbase/blob/50bce440277d4383dcf3a055cb9b5d513735bcbc/src/plugins/platforms/xcb/qxcbwindow.cpp#L1213-L1216">from Qt</a>
    /// and <a href="https://github.com/glfw/glfw/issues/1236">this GLFW issue</a>.
    /// </remarks>
    public static void EnableMousePassthrough(void* dpy, uint win)
    {
        var conn = XCB.Xlib_XGetXCBConnection(dpy);
        
        var region = XCB.GenerateID(conn);
        XCB.XFixesCreateRegion(conn, region, 0, null);
        XCB.XFixesSetWindowShapeRegionChecked(conn, win, XCB.ShapeSK.Input, 0, 0, region);
        XCB.XFixesDestroyRegion(conn, region);
    }
}