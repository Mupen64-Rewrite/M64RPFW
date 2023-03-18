using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Controls;
using M64RPFW.Views.Avalonia.Services;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : Window, IDispatcherService, IWindowDimensionsService
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        MainViewModel = new MainViewModel(this, new FilesService(), this.Find<VidextControl>("EmulatorWindow"), this);
    
        DataContext = this;
    }

    public MainViewModel MainViewModel { get; }

    public void Execute(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }

    private void ShowSettingsWindowMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsDialog();
        dialog.ShowDialog(this);
    }

    public event Action<(double Width, double Height)>? DimensionsChanged;

    double IWindowDimensionsService.MenuHeight => MainMenu.Height;

    bool IWindowDimensionsService.IsResizable
    {
        set => CanResize = value;
    }

    private void MainWindow_OnDimensionsChanged((double Width, double Height) obj)
    {
        DimensionsChanged?.Invoke(obj);
    }
}