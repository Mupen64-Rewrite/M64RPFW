﻿using System;
using Eto.Forms;
using M64RPFW;
using M64RPFW.Views;

namespace M64PRR.Mac
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Eto.Platforms.Mac64).Run(new MainView());
        }
    }
}