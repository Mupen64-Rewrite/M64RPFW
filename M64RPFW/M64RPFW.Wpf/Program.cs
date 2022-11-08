using System;
using Eto.Forms;
using M64RPFW.Controls;
using M64RPFW.Misc;

namespace M64PRR.Wpf
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.Wpf.Platform();
            platform.Add<GLSubWindow.IGLSubWindow>(() => new M64RPFW.Wpf.Controls.GLSubWindow());

            Console.WriteLine("Test!");
            new M64RPFWApplication(platform).Run();
        }
    }
}