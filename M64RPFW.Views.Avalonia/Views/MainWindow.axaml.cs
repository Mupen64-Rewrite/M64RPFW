using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using M64RPFW.Models.Emulation;
using M64RPFW.Models.Types;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Controls;
using M64RPFW.Views.Avalonia.Services;
using Size = System.Drawing.Size;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : Window, IWindowSizingService
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        _vidextControl = this.Find<VidextControl>("EmulatorWindow")!;
        
        EmulatorViewModel = new EmulatorViewModel(this.Find<VidextControl>("EmulatorWindow")!, (App) Application.Current!,
            FilePickerService.Instance, this);
        SettingsViewModel = new SettingsViewModel(FilePickerService.Instance);

        DataContext = this;

        _shouldBlockSizeChangeEvents = false;
    }
    
    // avalonia compiled binding resolver lives in another assembly, so these have to be public :(
    public EmulatorViewModel EmulatorViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    private void Window_OnClosed(object? sender, EventArgs eventArgs)
    {
        EmulatorViewModel.OnWindowClosed();
    }
    
    private void Window_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (!_shouldBlockSizeChangeEvents)
            EmulatorViewModel.OnSizeChanged();
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

    private void ShowSettingsDialogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsDialog
        {
            DataContext = SettingsViewModel
        };
        dialog.ShowDialog(this);
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

    private VidextControl _vidextControl;
    private bool _shouldBlockSizeChangeEvents;
    private double _prevVidextMinWidth, _prevVidextMinHeight;
}