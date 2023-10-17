using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using Avalonia.Threading;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Markup;
using M64RPFW.Views.Avalonia.Views;

namespace M64RPFW.Views.Avalonia;

public class App : Application, IDispatcherService
{
    private readonly Styles _themeStylesContainer = new();
    private FluentTheme FluentTheme => (FluentTheme)Resources["FluentTheme"]!;
    private SimpleTheme SimpleTheme => (SimpleTheme)Resources["SimpleTheme"]!;
    private IStyle DataGridFluent => (IStyle)Resources["DataGridFluent"]!;
    private IStyle DataGridSimple => (IStyle)Resources["DataGridSimple"]!;
    private bool _firstSetTheme = true;

    public override void Initialize()
    {
        Styles.Add(_themeStylesContainer);

        AvaloniaXamlLoader.Load(this);
        Name = "M64RPFW";

        // handling "special-case" settings here or in the SettingsDialog code-behind doesn't make a big difference
        // (except that here we don't have to worry about leaking due to strong refs)
        // we would have to break single-responsibility anyway
        SettingsViewModel.Instance.PropertyChanged += (_, args) =>
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
                    Current!.RequestedThemeVariant = SettingsViewModel.Instance.Theme switch
                    {
                        "Dark" => ThemeVariant.Dark,
                        "Light" => ThemeVariant.Light,
                        _ => ThemeVariant.Default
                    };
                    break;
                case nameof(SettingsViewModel.Style):
                    SetTheme(SettingsViewModel.Instance.Style);
                    break;
            }
        };

        // HACK: invoke PropertyChanged on every prop in settings vm at first initialization (see m64rpfw wpf branch) 
        // anything which depends on PropertyChanged events from settings vm will be refreshed
        // this is only necessary at first initialization, to make the program get its shit together
        SettingsViewModel.Instance.NotifyAllPropertiesChanged();
    }

    private void SetTheme(string theme)
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            throw new ApplicationException();

        if (!_firstSetTheme)
            return;
        
        if (_themeStylesContainer.Count == 0)
        {
            _themeStylesContainer.Add(new Style());
            _themeStylesContainer.Add(new Style());
        }

        if (theme == "Fluent")
        {
            _themeStylesContainer[0] = FluentTheme!;
            _themeStylesContainer[1] = DataGridFluent!;
        }
        else
        {
            _themeStylesContainer[0] = SimpleTheme!;
            _themeStylesContainer[1] = DataGridSimple!;
        }
        _firstSetTheme = false;
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

    public void ExecuteAndWait(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }

}