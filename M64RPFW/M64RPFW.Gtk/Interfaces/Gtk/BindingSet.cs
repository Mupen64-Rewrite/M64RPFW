using System;
using System.Reflection;
using System.Runtime.InteropServices;
using GLib;
using Gtk;

namespace M64RPFW.Gtk.Interfaces.Gtk;

/// <summary>
/// Wraps GtkBindingSet and GtkBindingEntry.
/// This is by no means a complete wrapper, but it does the job
/// for preventing hotkey overrides.
/// </summary>
public class BindingSet
{
    #region DLL Imports

    private const string LibName = "libgtk-3.so";

    [DllImport(LibName)]
    private static extern IntPtr gtk_binding_set_by_class(IntPtr classInstance);
    
    [DllImport(LibName)]
    private static extern void gtk_binding_entry_remove(IntPtr bindingSet, Gdk.Key keyVal, Gdk.ModifierType modifiers);
    
    #endregion
    
    
    public IntPtr Handle { get; }

    public BindingSet(IntPtr raw)
    {
        Handle = raw;
    }

    public static BindingSet ByClass<T>() where T : GLib.Object
    {
        var typeMember = typeof(T).GetProperty("GType");
        if (typeMember is null)
            throw new ArgumentException("T doesn't have a property GType", nameof(T));

        GType type = (GType) typeMember.GetValue(null)!;
        return new BindingSet(gtk_binding_set_by_class(type.GetClassPtr()));
    }

    public void RemoveEntry(Gdk.Key keyVal, Gdk.ModifierType modifiers = 0)
    {
        gtk_binding_entry_remove(Handle, keyVal, modifiers);
    }
    
    
}