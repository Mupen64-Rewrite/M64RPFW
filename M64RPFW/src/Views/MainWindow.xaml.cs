using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Extensions.Bindings;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.ViewModels;
using M64RPFW.ViewModels.Containers;
using ModernWpf;
using File = System.IO.File;

namespace M64RPFW.Views;

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
    IBitmapsService,
    IDispatcherService,
    IApplicationClosingEventService
{
    private const string LocalSettingsPath = "appsettings.json";

    private SettingsWindow? _settingsWindow;

    public MainWindow()
    {
        LocalizationService = this;

        // load settings from file or create from defaults
        try
        {
            LocalSettings = LocalSettings.FromJson(File.ReadAllText(LocalSettingsPath));
        }
        catch
        {
            Debug.WriteLine("Failed to load settings, falling back to defaults...");
            LocalSettings = LocalSettings.Default;
        }

        InitializeComponent();

        FilesService = new FilesService();


        var generalDependencyContainer =
            new GeneralDependencyContainer(this, this, this, this, FilesService, this);


        SettingsViewModel = new SettingsViewModel(generalDependencyContainer, LocalSettings);
        MainViewModel = new MainViewModel(generalDependencyContainer, SettingsViewModel);

        LocalSettings.OnSettingChanged += delegate(object? sender, string key)
        {
            if (key == nameof(SettingsViewModel.Theme))
            {
                if (SettingsViewModel.Theme.Equals("Light", StringComparison.InvariantCultureIgnoreCase))
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                else if (SettingsViewModel.Theme.Equals("Dark", StringComparison.InvariantCultureIgnoreCase))
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                else
                    throw new ArgumentException("Couldn't resolve theme");
            }

            if (key == nameof(SettingsViewModel.Culture))
                (this as IDispatcherService).Execute(delegate
                {
                    var culture = CultureInfo.GetCultureInfo(SettingsViewModel.Culture);
                    Thread.CurrentThread.CurrentCulture =
                        Thread.CurrentThread.CurrentUICulture =
                            LocalizationSource.Instance.CurrentCulture = culture;
                });
        };

        LocalSettings.InvokeOnSettingChangedForAllKeys();

        DataContext = this;
    }

    internal static LocalSettings LocalSettings { get; private set; }
    internal static ILocalizationService LocalizationService { get; private set; }
    internal static FilesService FilesService { get; private set; }

    public MainViewModel MainViewModel { get; private set; }
    public SettingsViewModel SettingsViewModel { get; }

    public event Action? OnApplicationClosing;

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        OnApplicationClosing?.Invoke();
        File.WriteAllText(LocalSettingsPath, LocalSettings.ToJson());
    }

    #region Service Method Implementations

    public void ShowError(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _ = MessageBox.Show(message, (this as ILocalizationService).GetStringOrDefault("Error"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        });
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

    IBitmap IBitmapsService.Create(IBitmapsService.BitmapTargets bitmapTarget, int width, int height)
    {
        Trace.Assert(Thread.CurrentThread.ManagedThreadId == Dispatcher.Thread.ManagedThreadId);

        Bitmap bitmap;

        if (bitmapTarget == IBitmapsService.BitmapTargets.Game)
            bitmap = new Bitmap(Main_Image, width, height);
        else
            throw new InvalidEnumArgumentException($"Couldn't resolve target \"{bitmapTarget}\"");

        return bitmap;
    }


    // void IBitmapsService.Create(int width, int height)
    // {
    //     
    //     Application.Current.Dispatcher.Invoke(delegate
    //     {
    //         _gameWriteableBitmap = new WriteableBitmap(width, height, VisualTreeHelper.GetDpi(this).PixelsPerInchX,
    //             VisualTreeHelper.GetDpi(this).PixelsPerInchY, PixelFormats.Cmyk32, null);
    //         //Main_Image.Source = _gameWriteableBitmap;
    //     });
    // }
    //
    // void IBitmapsService.Draw(Array buffer, int width, int height)
    // {
    //     Application.Current.Dispatcher.Invoke(delegate
    //     {
    //         Trace.Assert(_gameWriteableBitmap != null, nameof(_gameWriteableBitmap) + " != null");
    //         _gameWriteableBitmap.WritePixels(new Int32Rect(0, 0, width, height), buffer, width * sizeof(int), 0);
    //     });
    // }

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
        _settingsWindow = new SettingsWindow { DataContext = SettingsViewModel };
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
}