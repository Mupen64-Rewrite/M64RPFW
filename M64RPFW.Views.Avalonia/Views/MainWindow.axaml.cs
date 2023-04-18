using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Controls;
using M64RPFW.Views.Avalonia.Controls.Helpers;
using M64RPFW.Views.Avalonia.MarkupExtensions;
using M64RPFW.Views.Avalonia.Services;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : Window, IWindowSizingService, IViewDialogService
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);


        _vidextControl = this.Find<VidextControl>("EmulatorWindow")!;

        DataContext = new EmulatorViewModel(this.Find<VidextControl>("EmulatorWindow")!, (App) Application.Current!,
            FilePickerService.Instance, this, this);

        _shouldBlockSizeChangeEvents = false;


        // handling "special-case" settings here or in the SettingsDialog code-behind doesn't make a big difference
        // (except that here we don't have to worry about leaking due to strong refs)
        // we would have to break single-responsibility anyway
        SettingsViewModel.Instance.PropertyChanged += (sender, args) =>
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

                    Application.Current!.RequestedThemeVariant =
                        SettingsViewModel.Instance.Theme.Equals("Dark", StringComparison.InvariantCultureIgnoreCase)
                            ? ThemeVariant.Dark
                            : ThemeVariant.Light;

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
        if (!_shouldBlockSizeChangeEvents)
        {
            ViewModel.OnSizeChanged();
        }
        else
        {
            if (CanResize)
            {
                _vidextControl.MinHeight = _prevVidextMinHeight;
                _vidextControl.MinWidth = _prevVidextMinWidth;
            }

            _shouldBlockSizeChangeEvents = false;
        }
    }

    public WindowSize GetWindowSize()
    {
        return new WindowSize(_vidextControl.Width, _vidextControl.Height);
    }

    public void LayoutToFit(WindowSize size)
    {
        _shouldBlockSizeChangeEvents = true;
        _prevVidextMinWidth = _vidextControl.MinWidth;
        _prevVidextMinHeight = _vidextControl.MinHeight;
        _vidextControl.MinWidth = size.Width;
        _vidextControl.MinHeight = size.Height;
        SizeToContent = SizeToContent.WidthAndHeight;
    }

    private readonly VidextControl _vidextControl;
    private bool _shouldBlockSizeChangeEvents;
    private double _prevVidextMinWidth, _prevVidextMinHeight;

    public Task ShowSettingsDialog()
    {
        SettingsDialog d = new();
        return d.ShowDialog(this);
    }

    public Task ShowAdvancedSettingsDialog()
    {
        AdvancedSettingsDialog d = new();
        return d.ShowDialog(this);
    }

    public Task<OpenMovieDialogResult?> ShowOpenMovieDialog(bool paramsEditable)
    {
        OpenMovieDialog d = new();
        d.ViewModel.IsEditable = paramsEditable;
        return d.ShowDialog<OpenMovieDialogResult?>(this);
    }

    private void Window_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var scancode = SDLHelpers.ToSDLScancode(e.Key);
        var modifiers = SDLHelpers.ToSDLKeymod(e.KeyModifiers);
        ViewModel.ForwardSDLKeyDown(scancode, modifiers);
    }

    private void Window_OnKeyUp(object? sender, KeyEventArgs e)
    {
        var scancode = SDLHelpers.ToSDLScancode(e.Key);
        var modifiers = SDLHelpers.ToSDLKeymod(e.KeyModifiers);
        ViewModel.ForwardSDLKeyUp(scancode, modifiers);
    }
}