using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Eto.Forms;
using Gdk;
using M64RPFW.Controls;
using M64RPFW.Gtk.Interfaces.Gtk;
using M64RPFW.Misc;
using M64RPFW.Presenters.Helpers;
using Window = Eto.Forms.Window;
using Gtk = global::Gtk;

namespace M64RPFW.Gtk
{
    class Program
    {
        // undoes add_tab_bindings in gtkwindow.c
        private static void RemoveTabBindings(BindingSet bindingSet, ModifierType modifiers)
        {
            bindingSet.RemoveEntry(Key.Tab, modifiers);
            bindingSet.RemoveEntry(Key.KP_Tab, modifiers);
        }
        
        // undoes add_arrow_bindings in gtkwindow.c
        private static void RemoveArrowBindings(BindingSet bindingSet, Key keySym)
        {
            const uint keypad_diff =  (uint) Key.KP_Left - (uint) Key.Left;

            Key keypadSym = (Key) ((uint) keySym + keypad_diff);
            
            bindingSet.RemoveEntry(keySym);
            bindingSet.RemoveEntry(keySym, ModifierType.ControlMask);
            bindingSet.RemoveEntry(keypadSym);
            bindingSet.RemoveEntry(keypadSym, ModifierType.ControlMask);
        }
        
        /// <summary>
        /// Undoes every default key binding on <see cref="global::Gtk.Window"/>.
        /// </summary>
        private static void RemoveAllWindowBindings()
        {
            // Get the binding set for GtkWindow
            BindingSet set = BindingSet.ByClass<global::Gtk.Window>();
                
            Console.WriteLine("Bruh");
                
            // Enter and space
            set.RemoveEntry(Key.space);
            set.RemoveEntry(Key.KP_Space);
                
            set.RemoveEntry(Key.Return);
            set.RemoveEntry(Key.ISO_Enter);
            set.RemoveEntry(Key.KP_Enter);
                
            // Debugging shortcuts: Ctrl+Shift+I, Ctrl+Shift+D
            set.RemoveEntry(Key.I, ModifierType.ControlMask | ModifierType.ShiftMask);
            set.RemoveEntry(Key.D, ModifierType.ControlMask | ModifierType.ShiftMask);
                
            // Arrow shortcuts
            RemoveArrowBindings(set, Key.Up);
            RemoveArrowBindings(set, Key.Down);
            RemoveArrowBindings(set, Key.Left);
            RemoveArrowBindings(set, Key.Right);
                
            // Tab shortcuts
            RemoveTabBindings(set, ModifierType.None);
            RemoveTabBindings(set, ModifierType.ShiftMask);
            RemoveTabBindings(set, ModifierType.ControlMask);
            RemoveTabBindings(set, ModifierType.ControlMask | ModifierType.ShiftMask);
        }

        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.GtkSharp.Platform();
            // Register our custom control
            platform.Add<GLSubWindow.IGLSubWindow>(() => new M64RPFW.Gtk.Controls.GLSubWindow());
            
            RemoveAllWindowBindings();

            new M64RPFWApplication(platform).Run();
        }
    }
}