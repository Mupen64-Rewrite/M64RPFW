using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using M64RPFW.Controls;
using M64RPFW.Misc;

namespace M64RPFW.Wpf
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.Wpf.Platform();
            platform.Add<GLSubWindow.IGLSubWindow>(() => new M64RPFW.Wpf.Controls.GLSubWindow());

            PInvoke.AttachConsole(0xFFFF_FFFF);

            new M64RPFWApplication(platform).Run();
        }
    }
}