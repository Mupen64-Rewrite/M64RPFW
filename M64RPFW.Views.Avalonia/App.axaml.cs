using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using M64RPFW.Services;
using M64RPFW.Views.Avalonia.Views;

namespace M64RPFW.Views.Avalonia;

public class App : Application, IDispatcherService
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Name = "M64RPFW";
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