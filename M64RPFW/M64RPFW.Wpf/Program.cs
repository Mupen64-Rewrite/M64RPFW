using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using M64RPFW.Controls;
using M64RPFW.Misc;

namespace M64PRR.Wpf
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Console.WriteLine($"Exception thrown! ({eventArgs.ExceptionObject.GetType().FullName})");
                Console.WriteLine(eventArgs.ExceptionObject.ToString());
            };

            var platform = new Eto.Wpf.Platform();
            platform.Add<GLSubWindow.IGLSubWindow>(() => new M64RPFW.Wpf.Controls.GLSubWindow());

            PInvoke.AttachConsole(0xFFFF_FFFF);

            Console.WriteLine("Test!");
            try
            {
                new M64RPFWApplication(platform).Run();
            }
            catch (SEHException e)
            {
                Console.WriteLine($"Segfault!");
            }

            Console.ReadKey();
        }
    }
}