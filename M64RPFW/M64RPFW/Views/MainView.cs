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
                        new SeparatorMenuItem(),
                        new RelayWrapperCommand(Presenter.ShowSettingsCommand)
                        {
                            MenuText = "Settings..."
                        }
                    }
                },
                new SubMenuItem
                {
                    Text = "&Emulation", Items =
                    {
                        FormCommandHelper.DoPostInit(new CheckCommand
                        {
                            MenuText = "Pause/Resume"
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
                        new RelayWrapperCommand(Presenter.FrameAdvanceCommand)
                        {
                            MenuText = "Frame Advance"
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