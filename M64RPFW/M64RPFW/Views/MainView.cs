using System;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Controls;
using M64RPFW.Misc;
using M64RPFW.Presenters;
using static M64RPFW.Views.Helpers.FormCommandHelper;

// ReSharper disable VariableHidesOuterVariable

namespace M64RPFW.Views;

public partial class MainView : Form
{
    public MainView()
    {
        DataContext = new MainPresenter(this);

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
                    Text = "&Emulation",
                    Items =
                    {
                        new CheckMenuItem
                        {
                            Text = "Pause/Resume",
                            Command = Presenter.PauseStateCommand
                        },
                        new ButtonMenuItem
                        {
                            Text = "Frame advance",
                            Command = Presenter.FrameAdvanceCommand
                        },
                        new SeparatorMenuItem(),
                        new ButtonMenuItem
                        {
                            Text = "Load from file",
                            Command = Presenter.LoadSavestateFileCommand
                        },
                        new ButtonMenuItem
                        {
                            Text = "Save to file",
                            Command = Presenter.SaveSavestateFileCommand
                        },
                        new ButtonMenuItem
                        {
                            Text = "Load from slot",
                            Command = Presenter.LoadSlotCommand
                        },
                        new ButtonMenuItem
                        {
                            Text = "Save to slot",
                            Command = Presenter.SaveSlotCommand
                        },
                        DoPostInit(new SubMenuItem
                        {
                            Text = "Save slots",
                            ID = "save-slot-menu",
                            Items =
                            {
                                DoPostInit(new RadioMenuItem
                                {
                                    Text = "0",
                                    CommandParameter = 0,
                                    Command = Presenter.SetSavestateSlotCommand
                                }, item => { MenuSavestateController = item; }),
                                new RadioMenuItem(MenuSavestateController)
                                {
                                    Text = "1",
                                    CommandParameter = 1,
                                    Command = Presenter.SetSavestateSlotCommand
                                },
                                new RadioMenuItem(MenuSavestateController)
                                {
                                    Text = "2",
                                    CommandParameter = 2,
                                    Command = Presenter.SetSavestateSlotCommand
                                },
                                new RadioMenuItem(MenuSavestateController)
                                {
                                    Text = "3",
                                    CommandParameter = 3,
                                    Command = Presenter.SetSavestateSlotCommand
                                },
                                new RadioMenuItem(MenuSavestateController)
                                {
                                    Text = "4",
                                    CommandParameter = 4,
                                    Command = Presenter.SetSavestateSlotCommand
                                },
                                new RadioMenuItem(MenuSavestateController)
                                {
                                    Text = "5",
                                    CommandParameter = 5,
                                    Command = Presenter.SetSavestateSlotCommand
                                },
                                new RadioMenuItem(MenuSavestateController)
                                {
                                    Text = "6",
                                    CommandParameter = 6,
                                    Command = Presenter.SetSavestateSlotCommand
                                },
                                new RadioMenuItem(MenuSavestateController)
                                {
                                    Text = "7",
                                    CommandParameter = 7,
                                    Command = Presenter.SetSavestateSlotCommand
                                },
                                new RadioMenuItem(MenuSavestateController)
                                {
                                    Text = "8",
                                    CommandParameter = 8,
                                    Command = Presenter.SetSavestateSlotCommand
                                },
                                new RadioMenuItem(MenuSavestateController)
                                {
                                    Text = "9",
                                    CommandParameter = 9,
                                    Command = Presenter.SetSavestateSlotCommand
                                },
                            }
                        }, subMenu =>
                        {
                            var enableBinding = subMenu.BindDataContext(
                                m => m.Enabled, (MainPresenter p) => p.IsNotStopped, DualBindingMode.OneWay);

                            Presenter.EmuStateChanged += (_, _) =>
                            {
                                enableBinding.Update();
                            };
                        })
                    }
                }
            }
        };

        Content = RomView;
        
        Presenter.PostInit();
    }

    internal MainPresenter Presenter => (MainPresenter) DataContext;

    // Components
    internal RecentRomView RomView { get; }
    internal GLSubWindow SubWindow { get; }

    private RadioMenuItem MenuSavestateController { set; get; }
}