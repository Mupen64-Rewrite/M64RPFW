using System;
using M64RPFW.Controls;
using M64RPFW.Misc;

namespace M64RPFW.Gtk
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.GtkSharp.Platform();
            // Register our custom control
            platform.Add<GLSubWindow.IGLSubWindow>(() => new M64RPFW.Gtk.Controls.GLSubWindow());
            
            new M64RPFWApplication(platform).Run();
        }
    }
}