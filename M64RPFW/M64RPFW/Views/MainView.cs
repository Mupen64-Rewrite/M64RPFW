using System;
using System.IO;
using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Controls;
using M64RPFW.Misc;
using M64RPFW.Models;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Presenters;

namespace M64RPFW.Views;

public partial class MainView : Form
{

    public MainView()
    {
        Presenter = new MainPresenter(this);

        RomView = new RecentRomView(this);
        SubWindow = new GLSubWindow();
        
        ClientSize = new Size(640, 480);
        Title = "Mupen64Plus-RR";

        // create a few commands that can be used for the menu and toolbar
        var testCommand = new Command { MenuText = "Test" };
        testCommand.Executed += (_, _) => Console.WriteLine("Test called");

            // create menu
        Menu = new MenuBar
        {
            Items =
            {
                // File submenu
                new SubMenuItem { Text = "&File", Items = { testCommand } },
                new SubMenuItem { Text = "&Emulator", Items =
                {
                    new ForwardingRelayCommand(Presenter.StopCommand)
                } },
                // new SubMenuItem { Text = "&View", Items = { /* commands/items */ } },
            }
        };

        Content = RomView;
    }

    internal MainPresenter Presenter { get; }
    internal RecentRomView RomView { get; }
    internal GLSubWindow SubWindow { get; }
}