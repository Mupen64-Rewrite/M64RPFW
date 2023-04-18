using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
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
            var win = new MainWindow();
            desktop.MainWindow = win;
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    

    public void Execute(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
    
}