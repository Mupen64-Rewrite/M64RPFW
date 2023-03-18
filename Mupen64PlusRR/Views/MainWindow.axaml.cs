using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Mupen64PlusRR.Controls;

namespace Mupen64PlusRR.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        
        var serviceCollection = ((App)Application.Current!).ServiceProvider.GetService<IServiceCollection>();
        
        serviceCollection.AddSingleton<IOpenGLContextService>(this.Find<VidextControl>("EmulatorWindow"));
        
        ((App)Application.Current!).ServiceProvider = serviceCollection.BuildServiceProvider();
        
        DataContext = ((App)Application.Current!).ServiceProvider.GetService<MainViewModel>().EmulatorViewModel;
    }

    private EmulatorViewModel ViewModel => (EmulatorViewModel) DataContext!;

    private void Window_OnClosed(object? sender, EventArgs eventArgs)
    {
        ViewModel.OnWindowClosed();
    }
}