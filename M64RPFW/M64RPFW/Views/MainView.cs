using System;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Controls;
using M64RPFW.Misc;
using M64RPFW.Presenters;

namespace M64RPFW.Views;

public partial class MainView : Form
{
    public MainView()
    {
        Presenter = new MainPresenter(this);
        DataContext = Presenter;

        RomView = new RecentRomView(this);
        SubWindow = new GLSubWindow();

        ClientSize = new Size(640, 480);
        MinimumSize = new Size(256, 144);
        Title = "Mupen64Plus-RR";

        // initialize

        // create menu
        Menu = new MenuBar
        {
            Items =
            {
                // File submenu
                new SubMenuItem
                {
                    Text = "&File", Items =
                    {
                        new RelayWrapperCommand(Presenter.OpenRomCommand)
                        {
                            MenuText = "Load ROM..."
                        },
                        new RelayWrapperCommand(Presenter.CloseRomCommand)
                        {
                            MenuText = "Close ROM"
                        },
                        new RelayWrapperCommand(Presenter.ResetCommand)
                        {
                            MenuText = "Reset ROM"
                        },
                        new SeparatorMenuItem()
                        // Insert settings menu here
                    }
                },
                new SubMenuItem
                {
                    Text = "&Emulation", Items =
                    {
                        FormCommandHelper.DoPostInit(new CheckCommand
                        {
                            MenuText = "Pause/Resume"
                        }, c =>
                        {
                            // There are no declarative bindings like Avalonia, so I need to do this.
                            
                            // init bindings
                            var checkedBinding = c.Bind(new PropertyBinding<bool>("Checked"), Presenter,
                                new PropertyBinding<bool>("PauseState"));
                            var enabledBinding = c.Bind(new PropertyBinding<bool>("Enabled"), Presenter,
                                new PropertyBinding<bool>("IsNotStopped"));
                            
                            // Update them when the emulator state changes
                            Presenter.EmuStateChanged += (_, _) =>
                            {
                                checkedBinding.Update();
                                enabledBinding.Update();
                            };
                        }),
                        new RelayWrapperCommand(Presenter.FrameAdvanceCommand)
                        {
                            MenuText = "Frame Advance"
                        }
                    }
                },
                // new SubMenuItem { Text = "&View", Items = { /* commands/items */ } },
            }
        };

        Content = RomView;
    }

    internal MainPresenter Presenter { get; }
    
    // Components
    internal RecentRomView RomView { get; }
    internal GLSubWindow SubWindow { get; }
}