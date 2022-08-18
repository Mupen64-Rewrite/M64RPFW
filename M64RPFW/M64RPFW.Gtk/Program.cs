using System;
using Eto.Forms;
using Gdk;
using Gtk;
using M64RPFW.Controls;
using M64RPFW.Misc;
using M64RPFW.Presenters.Helpers;
using Window = Eto.Forms.Window;

namespace M64RPFW.Gtk
{
    class Program
    {
        private static void RegisterWindowKeyHandlers(Window window, EventHandler<KeyEventArgs> down,
            EventHandler<KeyEventArgs> up)
        {
            window.KeyDown += down;
            window.KeyUp += up;

            var gtkWidget = (global::Gtk.Window) window.ToNative();
            gtkWidget.DefaultActivated += (_, eventArgs) =>
            {
                down(window, new KeyEventArgs(Keys.Enter, KeyEventType.KeyDown, '\n'));
            };
            
            gtkWidget.CanDefault = true;
            gtkWidget.GrabDefault();
        }
        
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.GtkSharp.Platform();
            // Register our custom control
            platform.Add<GLSubWindow.IGLSubWindow>(() => new M64RPFW.Gtk.Controls.GLSubWindow());
            
            // Register view helpers
            ViewInitHelpers.RegisterWindowKeyHandlersImpl = RegisterWindowKeyHandlers;

            new M64RPFWApplication(platform).Run();
        }
    }
}