using System;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Controls;
using M64RPFW.Misc;
using M64RPFW.Presenters;
// ReSharper disable VariableHidesOuterVariable

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
                        new ButtonMenuItem
                        {
                            Text = "Load ROM...",
                            Command = Presenter.OpenRomCommand
                        },
                        new ButtonMenuItem
                        {
                            Text = "Close ROM",
                            Command = Presenter.CloseRomCommand
                        },
                        new ButtonMenuItem
                        {
                            Text = "Reset ROM",
                            Command = Presenter.ResetCommand
                        },
                        new SeparatorMenuItem(),
                        new ButtonMenuItem
                        {
                            Text = "Settings",
                            Command = Presenter.ShowSettingsCommand
                        }
                    }
                },
                new SubMenuItem
                {
                    Text = "&Emulation", Items =
                    {
                        new CheckMenuItem
                        {
                            Text = "Pause/Resume",
                            Command = Presenter.PauseStateCommand
                        },
                        new ButtonMenuItem
                        {
                            Text = "Frame Advance",
                            Command = Presenter.FrameAdvanceCommand
                        },
                        new SeparatorMenuItem(),
                        new RadioMenuItem(),
                    }
                }
            }
        };

        Content = RomView;
    }

    internal MainPresenter Presenter { get; }

    // Components
    internal RecentRomView RomView { get; }
    internal GLSubWindow SubWindow { get; }
}