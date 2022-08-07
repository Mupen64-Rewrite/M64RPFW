using System;
using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Controls;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Views;
using OpenTK.Graphics.ES11;

namespace M64RPFW;

public partial class MainForm : Form
{
    public MainForm()
    {
        Title = "Mupen64Plus-RR";
        MinimumSize = new Size(200, 200);
        Content = new RecentRomView();

        // create a few commands that can be used for the menu and toolbar
        var testCommand = new Command { MenuText = "Test" };
        testCommand.Executed += (_, _) =>
        {
            Console.WriteLine("Test called");
        };

            // create menu
        Menu = new MenuBar
        {
            Items =
            {
                // File submenu
                new SubMenuItem { Text = "&File", Items = { testCommand } },
                // new SubMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
                // new SubMenuItem { Text = "&View", Items = { /* commands/items */ } },
            }
        };

        ClientSize = new Size(640, 480);
    }
}