using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Services;
using M64RPFW.src.Extensions.Bindings;
using M64RPFW.src.Services;
using M64RPFW.src.Settings;
using M64RPFW.ViewModels;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Helpers;
using ModernWpf;

namespace M64RPFW.src.Views;

/// <summary>
///     Code-behind for MainWindow.xaml.cs
///     <para></para>
///     <b>NOTE</b>:
///     <para></para>
///     This code-behind file does not perform any emulator-related tasks, computations or state management.
///     <para></para>
///     It implements various interfaces for this platform to be used by VMs and itself.
///     <para></para>
///     View-first MVVM is employed here: the View layer handles ViewModel creation
/// </summary>
public partial class MainWindow :
    Window,
    IDialogService,
    ILocalizationService,
    ISettingsService,
    IBitmapDrawingService,
    IDispatcherService,
    IApplicationClosingEventService
{
    internal static AppSettings AppSettings { get; private set; }
    internal static ILocalizationService LocalizationService { get; private set; }
    internal static FilesService FilesService { get; private set; }
    
    bool IBitmapDrawingService.IsReady => _writeableBitmap != null;
    public event Action? OnApplicationClosing;

    private const string AppSettingsPath = "appsettings.json";
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    private readonly MainViewModel _mainViewModel;

    private SettingsWindow? _settingsWindow;
    private WriteableBitmap _writeableBitmap;

    public MainWindow()
    {
        LocalizationService = this;

        // TODO: rewrite settings system (see Ambie)

        // load settings from file or create from defaults
        try
        {
            AppSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(AppSettingsPath));
            if (AppSettings == null)
            {
                throw new Exception();
            }
        }
        catch
        {
            Debug.WriteLine("Failed to load settings, falling back to defaults...");
            AppSettings = new AppSettings();
            Save();
        }
        
        InitializeComponent();

        FilesService = new();
        
        AppSettings.PropertyChanged += delegate(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(AppSettings.Theme))
            {
                if (AppSettings.Theme.Equals("Light", StringComparison.InvariantCultureIgnoreCase))
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                else if (AppSettings.Theme.Equals("Dark", StringComparison.InvariantCultureIgnoreCase))
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                else
                    throw new ArgumentException("Couldn't resolve theme");
            }

            if (args.PropertyName == nameof(AppSettings.Culture))
            {
                (this as IDispatcherService).Execute(delegate
                {
                    var culture = CultureInfo.GetCultureInfo(AppSettings.Culture);
                    Thread.CurrentThread.CurrentCulture =
                        Thread.CurrentThread.CurrentUICulture =
                            LocalizationSource.Instance.CurrentCulture = culture;
                });
            }
        };
        
        _generalDependencyContainer =
            new GeneralDependencyContainer(this, this, this, this, this, FilesService, this);

        _mainViewModel = new MainViewModel(_generalDependencyContainer);

        DataContext = _mainViewModel;
        
        AppSettings.NotifyAllPropertiesChanged();
    }
    
    #region Service Method Implementations

    public void ShowError(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _ = MessageBox.Show(message, (this as ILocalizationService).GetStringOrDefault("Error"), MessageBoxButton.OK,
                MessageBoxImage.Error);
        });
    }

    public T Get<T>(string key)
    {
        var prop = AppSettings.GetType().GetProperty(key);
        if (prop == null) throw new Exception($"Could not find property {key}");
        var val = (T)prop.GetValue(AppSettings);
        if (val == null) throw new Exception($"Could not get property value {key}");
        return val;
    }

    public void Set<T>(string key, T value, bool saveAfter = false)
    {
        var prop = AppSettings.GetType().GetProperty(key);
        prop.SetValue(AppSettings, value);
        if (saveAfter) Save();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(AppSettings, typeof(AppSettings),
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(AppSettingsPath, json);
    }

    public string? GetStringOrDefault(string key, string? @default = null)
    {
        try
        {
            return Properties.Resources.ResourceManager.GetString(key) ?? @default;
        }
        catch
        {
            return @default;
        }
    }

    void IBitmapDrawingService.Create(int width, int height)
    {
        Application.Current.Dispatcher.Invoke(delegate
        {
            _writeableBitmap = new WriteableBitmap(width, height, VisualTreeHelper.GetDpi(this).PixelsPerInchX,
                VisualTreeHelper.GetDpi(this).PixelsPerInchY, PixelFormats.Cmyk32, null);
            Main_Image.Source = _writeableBitmap;
        });
    }

    void IBitmapDrawingService.Draw(Array buffer, int width, int height)
    {
        Application.Current.Dispatcher.Invoke(delegate
        {
            _writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), buffer, width * sizeof(int), 0);
        });
    }

    void IDispatcherService.Execute(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }

    #endregion

    #region Windowing Commands

    [RelayCommand]
    private void ShowSettingsWindow()
    {
        // store reference and check against it being null because WPF accelerators are a dick and eat child window's events
        if (_settingsWindow != null) return;
        _settingsWindow = new SettingsWindow { DataContext = new SettingsViewModel(_generalDependencyContainer) };
        _ = _settingsWindow.ShowDialog();
        _settingsWindow = null; // this is okay, because ShowDialog() blocks
    }

    [RelayCommand]
    private void ShowRomInspectionWindow(object dataContext)
    {
        RomInspectionWindow romInspectionWindow = new() { DataContext = dataContext };
        romInspectionWindow.Show();
    }

    [RelayCommand]
    private void DoExit()
    {
        Close();
    }
    
    #endregion

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        OnApplicationClosing?.Invoke();
        Save();
    }

}