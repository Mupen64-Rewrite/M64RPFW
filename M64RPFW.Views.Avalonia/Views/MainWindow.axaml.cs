using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Controls.Helpers;
using M64RPFW.Views.Avalonia.Markup;
using M64RPFW.Views.Avalonia.Services;
using M64RPFW.Views.Avalonia.Extensions;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // since the main window has a finite lifetime, we need to recycle the emu vm
        if (EmulatorViewModel.Instance == null)
        {
            DataContext = new EmulatorViewModel(GlControl, (App)Application.Current!,
                FilePickerService.Instance, this, this);
        }
        else
        {
            DataContext = EmulatorViewModel.Instance;
        }

    }

    // avalonia compiled binding resolver lives in another assembly, so these have to be public :(
    public EmulatorViewModel ViewModel => (EmulatorViewModel)DataContext!;

    private void Window_OnClosed(object? sender, EventArgs eventArgs)
    {
        ViewModel.OnWindowClosed();
    }

    private void Window_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
    }

    private KeyGesture FastForwardKeyGesture => KeyGesture.Parse(SettingsViewModel.Instance.FastForwardHotkey);

    private void Window_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (FastForwardKeyGesture.Matches(e))
        {
            ViewModel.SetSpeedLimiterCommand.ExecuteIfPossible(false, false);
            e.Handled = true;
            return;
        }

        // This doesn't account for differences with e.Key, see
        // https://github.com/AvaloniaUI/Avalonia/issues/11797
        var scancode = SDLHelpers.ToSDLScancode(e.Key);
        var modifiers = SDLHelpers.ToSDLKeymod(e.KeyModifiers);
        ViewModel.ForwardSDLKeyDown(scancode, modifiers);
    }

    private void Window_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (FastForwardKeyGesture.Matches(e))
        {
            ViewModel.SetSpeedLimiterCommand.ExecuteIfPossible(true, true);
            e.Handled = true;
            return;
        }

        // This doesn't account for differences with e.Key, see
        // https://github.com/AvaloniaUI/Avalonia/issues/11797
        var scancode = SDLHelpers.ToSDLScancode(e.Key);
        var modifiers = SDLHelpers.ToSDLKeymod(e.KeyModifiers);
        ViewModel.ForwardSDLKeyUp(scancode, modifiers);
    }

    private void NewLuaInstance_OnClick(object? sender, RoutedEventArgs e)
    {
        var win = new LuaWindow();
        win.Show(this);
    }

    private void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        // add window keybindings automatically based off of all menuitems
        foreach (var menuItem in this.GetLogicalDescendants().OfType<MenuItem>())
        {
            if (menuItem.InputGesture == null || menuItem.Command == null)
            {
                Debug.Print($"{menuItem} has no input gesture or command, ignoring...");
                continue;
            }

#if DEBUG
            if (KeyBindings.Any(x => x.Command == menuItem.Command || x.Gesture == menuItem.InputGesture))
            {
                Debug.Print($"{menuItem.InputGesture} already registered");
            }
#endif
            KeyBindings.Add(new KeyBinding
            {
                Command = menuItem.Command,
                Gesture = menuItem.InputGesture
            });
        }
        RomBrowserControl.RomBrowserViewModel.RefreshCommand.ExecuteIfPossible();
    }

    private async void AboutAvalonia_OnClick(object? sender, RoutedEventArgs e)
    {
        var aboutDialog = new AboutAvaloniaDialog();
        await aboutDialog.ShowDialog(this);
    }

    private async void AboutRPFW_OnClick(object? sender, RoutedEventArgs e)
    {
        var aboutDialog = new AboutRPFWDialog();
        await aboutDialog.ShowDialog(this);
    }
}