using System;
using Eto.Forms;
using M64RPFW.Misc;

namespace M64PRR.Wpf
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.Wpf.Platform();
            Console.WriteLine("Test!");
            new M64RPFWApplication(platform).Run();
        }
    }
}