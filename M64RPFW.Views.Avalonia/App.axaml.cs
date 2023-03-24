using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Services;
using M64RPFW.Views.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using Mupen64PlusRR.Services;

namespace M64RPFW.Views.Avalonia;

public class App : Application, IDispatcherService
{
    internal const string LocalSettingsPath = "settings.json";
    public ServiceProvider ServiceProvider { get; set; } = null!;


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }
    
    


    public void ConfigureServices(IServiceCollection services)
    {
        LocalSettings? localSettings = null;
        try
        {
            localSettings = LocalSettings.FromJson(File.ReadAllText(LocalSettingsPath));
        }
        catch
        {
            // ignored
        }

        services.AddSingleton(services);

        services.AddSingleton<IDispatcherService>(this);
        services.AddSingleton<IFilesService>(new FilesService());
        services.AddSingleton<ILocalSettingsService>(localSettings ?? LocalSettings.Default);

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SettingsViewModel>();
    }


    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }

    public void Execute(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
}