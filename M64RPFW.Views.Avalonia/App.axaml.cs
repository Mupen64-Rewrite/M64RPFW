using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Services;
using M64RPFW.Views.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;

namespace M64RPFW.Views.Avalonia;

public partial class App : Application, IDispatcherService
{
    public ServiceProvider ServiceProvider { get; set; }

    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ServiceCollection services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IServiceCollection>(services);
        
        services.AddSingleton<IDispatcherService>(this);
        services.AddSingleton<IFilesService>(new FilesService());

        services.AddSingleton<MainViewModel>();
    }
    

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Execute(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
}