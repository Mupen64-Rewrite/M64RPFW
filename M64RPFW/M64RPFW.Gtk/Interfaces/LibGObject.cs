using System;
using System.Runtime.InteropServices;

namespace M64RPFW.Gtk.Interfaces;

public static class LibGObject
{
    private const string LibName = "libgobject-2.0.so";

    // DLL-imported functions
    // ==========================================

    [DllImport(LibName)]
    private static extern int g_type_check_instance_is_a(IntPtr instance, IntPtr type);


    // API
    // ==========================================

    /// <summary>
    /// Checks if <paramref name="obj"/> is an instance of <paramref name="type"/>.
    /// </summary>
    /// <param name="obj">The object instance to check</param>
    /// <param name="type">The type to check against</param>
    /// <returns>true if <paramref name="obj"/> is an instance of <paramref name="type"/></returns>
    public static unsafe bool GType_CheckInstanceType(GLib.Object obj, GLib.GType type)
    {
        if (obj.Handle == IntPtr.Zero)
            return false;

        // Equivalent to C: GType objGType = obj->g_class->g_type
        IntPtr objGType = **(IntPtr**) obj.Handle.ToPointer();
        IntPtr typeGType = type.Val;

        return objGType == typeGType || g_type_check_instance_is_a(obj.Handle, typeGType) != 0;
    }
}