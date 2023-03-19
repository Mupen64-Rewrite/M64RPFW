using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        
        var serviceCollection = ((App) Application.Current!).ServiceProvider.GetService<IServiceCollection>()!;
        // Add VidextControl as IOpenGLContextService
        serviceCollection.AddSingleton<IOpenGLContextService>(this.Find<VidextControl>("EmulatorWindow")!);
        // Initialize view model via constructor injection
        ((App) Application.Current).ServiceProvider = serviceCollection.BuildServiceProvider();
        DataContext = ((App) Application.Current).ServiceProvider.GetService<MainViewModel>()!.EmulatorViewModel;
    }

    private EmulatorViewModel ViewModel => (EmulatorViewModel) DataContext!;

    private void Window_OnClosed(object? sender, EventArgs eventArgs)
    {
        ViewModel.OnWindowClosed();
    }

    private void ShowSettingsDialogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsDialog();
        dialog.ShowDialog(this);
    }
}