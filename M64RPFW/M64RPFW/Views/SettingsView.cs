using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using M64RPFW.Models.Settings;
using M64RPFW.Presenters;
using static M64RPFW.Misc.FormCommandHelper;

namespace M64RPFW.Views;

public class SettingsView : Dialog
{
    public SettingsView()
    {
        Title = "Settings";
        ClientSize = new Size(360, 480);
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
                                            Console.WriteLine($"Called text binding: {(int) index}");
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
                                            Console.WriteLine($"Called tooltip binding: {(int) index}");
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
                                    Text = "Plugin dir",
                                    Padding = new Padding(5),
                                    Content = new StackLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Items =
                                        {
                                            new StackLayoutItem(new TextBox
                                            {
                                                PlaceholderText = "File path...",
                                                
                                            }, expand: true),
                                            new StackLayoutItem(new Button
                                            {
                                                Text = "Browse",
                                            }, expand: false)
                                        }
                                    }
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
}