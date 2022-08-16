using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Controls;
using M64RPFW.Models.Settings;
using M64RPFW.Presenters;
using static M64RPFW.Views.Helpers.FormCommandHelper;

namespace M64RPFW.Views;

public class SettingsView : Dialog
{
    public SettingsView()
    {
        Title = "Settings";
        Resizable = true;
            

        DataContext = new SettingsPresenter(this);
        Content = new TabControl
        {
            Pages =
            {
                new TabPage
                {
                    Text = "General",
                    Content = new Scrollable
                    {
                        Content = DoPostInit(new StackLayout
                        {
                            Padding = new Padding(5),
                            Spacing = 5,
                            Items =
                            {
                                new GroupBox
                                {
                                    Text = "Emulator type",
                                    Padding = new Padding(5, 5),
                                    Content = DoPostInit(new RadioButtonList
                                    {
                                        Orientation = Orientation.Vertical,
                                        Spacing = new Size(0, 5),
                                        ItemTextBinding = new DelegateBinding<object, string>(index =>
                                        {
                                            return (int) index switch
                                            {
                                                0 => "Pure Interpreter",
                                                1 => "Cached Interpreter",
                                                2 => "Dynamic Recompiler",
                                                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
                                            };
                                        }),
                                        ItemToolTipBinding = new DelegateBinding<object, string>(index =>
                                        {
                                            return (int) index switch
                                            {
                                                0 => "Directly interprets MIPS machine code. (most reliable)",
                                                1 => "Interprets MIPS machine code with some optimizations.",
                                                2 => "JITs MIPS machine code to x86(-64) machine code. (fastest)",
                                                _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
                                            };
                                        }),
                                        DataStore = Enumerable.Range(0, 3).Cast<object>()
                                    }, list =>
                                    {
                                        list.SelectedIndexBinding.Bind(
                                            () => (int) Settings.Core.R4300Emulator,
                                            value => Settings.Core.R4300Emulator =
                                                (CoreSettings.EmulatorType) value);
                                    })
                                },
                                DoPostInit(new CheckBox
                                    {
                                        Text = "Disable Expansion Pak memory",
                                        ToolTip = "May be needed for some games."
                                    },
                                    box =>
                                    {
                                        box.CheckedBinding.Bind(Settings.Core, settings => settings.DisableExtraMem);
                                    }),
                                DoPostInit(new CheckBox
                                    {
                                        Text = "Randomize interrupt timings",
                                        ToolTip = "Randomizes PI/SI interrupts."
                                    },
                                    box =>
                                    {
                                        box.CheckedBinding.Bind(Settings.Core, settings => settings.RandomizeInterrupt);
                                    }),
                                DoPostInit(new CheckBox
                                    {
                                        Text = "Auto-increment current savestate slot",
                                        ToolTip = "Changes the savestate slot every time you save"
                                    },
                                    box =>
                                    {
                                        box.CheckedBinding.Bind(Settings.Core,
                                            settings => settings.AutoStateSlotIncrement);
                                    })
                            }
                        }, layout =>
                        {
                            foreach (var item in layout.Items)
                                item.HorizontalAlignment = HorizontalAlignment.Stretch;
                        })
                    }
                },
                new TabPage
                {
                    Text = "Plugins",
                    Content = new Scrollable
                    {
                        Content = DoPostInit(new StackLayout
                        {
                            Padding = new Padding(5),
                            Spacing = 5,
                            Items =
                            {
                                new GroupBox
                                {
                                    Text = "Video plugin",
                                    Padding = new Padding(5),
                                    Content = DoPostInit(new CustomFilePicker
                                    {
                                        Filters = { SettingsPresenter.PluginFilter }
                                    }, picker =>
                                    {
                                        picker.CurrentPathBinding.Bind(Settings.RPFW.Plugins,
                                            settings => settings.Video);
                                    })
                                },
                                new GroupBox
                                {
                                    Text = "Audio plugin",
                                    Padding = new Padding(5),
                                    Content = DoPostInit(new CustomFilePicker
                                    {
                                        Filters = { SettingsPresenter.PluginFilter }
                                    }, picker =>
                                    {
                                        picker.CurrentPathBinding.Bind(Settings.RPFW.Plugins,
                                            settings => settings.Audio);
                                    })
                                },
                                new GroupBox
                                {
                                    Text = "Input plugin",
                                    Padding = new Padding(5),
                                    Content = DoPostInit(new CustomFilePicker
                                    {
                                        Filters = { SettingsPresenter.PluginFilter }
                                    }, picker =>
                                    {
                                        picker.CurrentPathBinding.Bind(Settings.RPFW.Plugins,
                                            settings => settings.Input);
                                    })
                                },
                                new GroupBox
                                {
                                    Text = "RSP plugin",
                                    Padding = new Padding(5),
                                    Content = DoPostInit(new CustomFilePicker
                                    {
                                        Filters = { SettingsPresenter.PluginFilter }
                                    }, picker =>
                                    {
                                        picker.CurrentPathBinding.Bind(Settings.RPFW.Plugins,
                                            settings => settings.RSP);
                                    })
                                }
                            }
                        }, layout =>
                        {
                            foreach (var item in layout.Items)
                                item.HorizontalAlignment = HorizontalAlignment.Stretch;
                        })
                    }
                }
            }
        };
    }

    private SettingsPresenter Presenter => (SettingsPresenter) DataContext;
}