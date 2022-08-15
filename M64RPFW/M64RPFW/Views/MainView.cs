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
                        FormCommandHelper.DoPostInit(new CheckCommand
                        {
                            MenuText = "Pause/Resume",
                            DataContext = Presenter
                        }, command =>
                        {
                            // init bindings
                            var checkedBinding = command.BindDataContext(
                                c => c.Checked, (MainPresenter p) => p.PauseState);
                            var enabledBinding = command.BindDataContext(
                                c => c.Enabled, (MainPresenter p) => p.IsNotStopped);

                            // Update them when the emulator state changes
                            Presenter.EmuStateChanged += (_, _) =>
                            {
                                checkedBinding.Update();
                                enabledBinding.Update();
                            };
                        }),
                        new ButtonMenuItem
                        {
                            Text = "Frame Advance",
                            Command = Presenter.FrameAdvanceCommand
                        },
                        new SeparatorMenuItem(),
                        new SubMenuItem
                        {
                            Text = "Save slots",
                            Items =
                            {
                                
                            }
                        }
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