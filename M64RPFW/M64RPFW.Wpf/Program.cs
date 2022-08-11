using System;
using Eto.Forms;
using M64RPFW.Views;

namespace M64PRR.Wpf
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Eto.Platforms.Wpf).Run(new MainView());
        }
    }
}