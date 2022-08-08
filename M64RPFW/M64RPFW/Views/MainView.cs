using System;
using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Presenters;
using M64RPFW.Views;

namespace M64RPFW;

public partial class MainView : Form
{
    public MainView()
    {
        Presenter = new MainPresenter(this);

        RomView = new RecentRomView();
        EmuView = new EmulatorView();
        
        Title = "Mupen64Plus-RR";
        Content = RomView;

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
                // new SubMenuItem { Text = "&Edit", Items = { /* commands/items */ } },
                // new SubMenuItem { Text = "&View", Items = { /* commands/items */ } },
            }
        };

        ClientSize = new Size(640, 480);
    }
    
    internal MainPresenter Presenter { get; }
    
    internal RecentRomView RomView { get; }
    internal EmulatorView EmuView { get; }
}