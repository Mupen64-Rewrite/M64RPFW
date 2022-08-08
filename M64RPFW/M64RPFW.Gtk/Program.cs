using System;
using Eto.Forms;
using M64RPFW.Controls;

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
            
            new Application(platform).Run(new MainView());
        }
    }
}