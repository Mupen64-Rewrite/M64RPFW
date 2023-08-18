using System;
using M64RPFW.Views.Avalonia.Helpers.Platform;

namespace M64RPFW.Views.Avalonia.Controls.Helpers.Platform;

internal static unsafe class X11Helpers
{
    private static XCB.Connection* _xcb;

    static X11Helpers()
    {
        if (!(OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()))
            return;

        _xcb = XCB.Connect(null, null);
        if (XCB.ConnectionHasError(_xcb) > XCB.ConnectionError.None)
        {
            XCB.Disconnect(_xcb);
            throw new Exception("XCB connection failed!!");
        }
    }

    public static void Disconnect()
    {
        if (_xcb == null)
            return;
        XCB.Disconnect(_xcb);
    }


    /// <summary>
    /// Enables mouse passthrough for a window.
    /// </summary>
    /// <param name="dpy">The Xlib <c>Display*</c>.</param>
    /// <param name="win">The XID of the window. </param>
    /// <remarks>
    /// The code here is taken <a href="https://github.com/qt/qtbase/blob/50bce440277d4383dcf3a055cb9b5d513735bcbc/src/plugins/platforms/xcb/qxcbwindow.cpp#L1213-L1216">from Qt</a>
    /// and <a href="https://github.com/glfw/glfw/issues/1236">this GLFW issue</a>.
    /// </remarks>
    public static void PlatformWindowSetup(uint win)
    {
        if (_xcb == null)
            throw new InvalidOperationException("XCB is not initialized. Check that your platform supports XCB.");

#if false
        // use XFixes protocol to make the window hit-test transparent
        var region = XCB.GenerateID(_xcb);
        XCB.XFixesCreateRegion(_xcb, region, 0, null);
        XCB.XFixesSetWindowShapeRegionChecked(_xcb, win, XCB.ShapeSK.Input, 0, 0, region);
        XCB.XFixesDestroyRegion(_xcb, region);
#endif

    }
}