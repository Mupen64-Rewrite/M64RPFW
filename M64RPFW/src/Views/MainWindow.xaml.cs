using System;
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
    IThemeService,
    ILocalizationService,
    ISettingsService,
    IBitmapDrawingService,
    IDispatcherService
{
    private const string AppSettingsPath = "appsettings.json";
    private readonly AppSettings _appSettings;
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    private readonly MainViewModel _mainViewModel;

    private SettingsWindow? _settingsWindow;
    private WriteableBitmap _writeableBitmap;

    public MainWindow()
    {
        LocalizationService = this;

        // todo: move appsettings logic to vm
        if (!File.Exists(AppSettingsPath))
        {
            // create settings
            _appSettings = new AppSettings();
            Save();
        }
        else
        {
            _appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(AppSettingsPath));
        }

        AppSettings = _appSettings;

        InitializeComponent();

        (this as ILocalizationService).SetLocale((this as ISettingsService).Get<string>("Culture"));
        (this as IThemeService).Set(Get<string>("Theme"));

        _generalDependencyContainer =
            new GeneralDependencyContainer(this, this, this, this, this, this, new FilesService());

        _mainViewModel = new MainViewModel(_generalDependencyContainer);

        DataContext = _mainViewModel;
    }

    internal static AppSettings AppSettings { get; private set; }
    internal static ILocalizationService LocalizationService { get; private set; }
    bool IBitmapDrawingService.IsReady => _writeableBitmap != null;

    #region Service Implementations

    public void ShowError(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _ = MessageBox.Show(message, (this as ILocalizationService).GetString("Error"), MessageBoxButton.OK,
                MessageBoxImage.Error);
        });
    }

    string IThemeService.Get()
    {
        return ((ISettingsService)this).Get<string>("Theme");
    }

    void IThemeService.Set(string themeName)
    {
        if (themeName.Equals("Light"))
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        else if (themeName.Equals("Dark")) ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
        Set("Theme", themeName, true);
    }

    public void SetLocale(string localeKey)
    {
        (this as IDispatcherService).Execute(delegate
        {
            var culture = CultureInfo.GetCultureInfo(localeKey);
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                    LocalizationSource.Instance.CurrentCulture = culture;
            Set("Culture", localeKey, true);
        });
    }


    public T Get<T>(string key)
    {
        var prop = _appSettings.GetType().GetProperty(key);
        if (prop == null) throw new Exception($"Could not find property {key}");
        var val = (T)prop.GetValue(_appSettings);
        if (val == null) throw new Exception($"Could not get property value {key}");
        return val;
    }

    public void Set<T>(string key, T value, bool saveAfter = false)
    {
        var prop = _appSettings.GetType().GetProperty(key);
        prop.SetValue(_appSettings, value);
        if (saveAfter) Save();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_appSettings, typeof(AppSettings),
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(AppSettingsPath, json);
    }

    public string GetString(string key)
    {
        return Properties.Resources.ResourceManager.GetString(key) ?? "?";
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

    [RelayCommand]
    private void AtExit()
    {
        _mainViewModel.ExitCommand.ExecuteIfPossible();
        Save();
    }

    #endregion
}