using System.ComponentModel;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Mupen64PlusRR.Controls;
using Mupen64PlusRR.Services;

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


    private void ShowSettingsDialogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        new SettingsDialog().ShowDialog(this);
    }

    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        File.WriteAllText(App.LocalSettingsPath,
            (((App)Application.Current!).ServiceProvider.GetService<ILocalSettingsService>() as LocalSettings)
            .ToJson());
    }
}