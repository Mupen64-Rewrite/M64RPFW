using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Styling;
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

        DataContext = new EmulatorViewModel(GlControl, (App) Application.Current!,
            FilePickerService.Instance, this, this);


        // handling "special-case" settings here or in the SettingsDialog code-behind doesn't make a big difference
        // (except that here we don't have to worry about leaking due to strong refs)
        // we would have to break single-responsibility anyway
        SettingsViewModel.Instance.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(SettingsViewModel.Culture):
                    try
                    {
                        LocalizationSource.Instance.CurrentCulture =
                            new CultureInfo(SettingsViewModel.Instance.Culture);
                    }
                    catch
                    {
                        // fall back to a default culture
                        // is this a good idea?
                        LocalizationSource.Instance.CurrentCulture =
                            new CultureInfo("en-US");
                    }

                    break;
                case nameof(SettingsViewModel.Theme):
                    
                    // Application.Current!.RequestedThemeVariant =
                    //     SettingsViewModel.Instance.Theme.Equals("Dark", StringComparison.InvariantCultureIgnoreCase)
                    //         ? ThemeVariant.Dark
                    //         : ThemeVariant.Light;
                    Application.Current!.RequestedThemeVariant = SettingsViewModel.Instance.Theme switch
                    {
                        "Dark" => ThemeVariant.Dark,
                        "Light" => ThemeVariant.Light,
                        _ => ThemeVariant.Default
                    };

                    break;
            }
        };

        // HACK: invoke PropertyChanged on every prop in settings vm at first initialization (see m64rpfw wpf branch) 
        // anything which depends on PropertyChanged events from settings vm will be refreshed
        // this is only necessary at first initialization, to make the program get its shit together
        SettingsViewModel.Instance.NotifyAllPropertiesChanged();
    }

    // avalonia compiled binding resolver lives in another assembly, so these have to be public :(
    public EmulatorViewModel ViewModel => (EmulatorViewModel) DataContext!;

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

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var win = new LuaWindow();
        win.Show(this);
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
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
    }
}