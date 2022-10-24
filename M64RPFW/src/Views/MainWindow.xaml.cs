using CommunityToolkit.Mvvm.Input;
using M64RPFW.src.Extensions.Localization;
using M64RPFW.ViewModels;
using M64RPFW.ViewModels.Configurations;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Interfaces;
using Microsoft.WindowsAPICodePack.Dialogs;
using ModernWpf;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace M64RPFW.src.Views
{
    /// <summary>
    /// Code-behind for MainWindow.xaml.cs 
    /// <para></para>
    /// <b>NOTE</b>:
    /// <para></para>
    /// This code-behind file does not perform any emulator-related tasks, computations or state management. <para></para>
    /// It implements various interfaces for this platform to be used by VMs and itself.
    /// </summary>
    public partial class MainWindow : Window, IFileDialogProvider,
        IDialogProvider,
        IRomFileExtensionsConfigurationProvider,
        ISavestateBoundsConfigurationProvider,
        IThemeManager,
        ILocalizationManager,
        ISettingsProvider,
        IDrawingSurfaceProvider,
        IUIThreadDispatcherProvider
    {
        private readonly MainViewModel mainViewModel;
        private readonly GeneralDependencyContainer generalDependencyContainer;
        private SettingsWindow? settingsWindow;

        SavestateBoundsConfiguration ISavestateBoundsConfigurationProvider.SavestateBoundsConfiguration => new();
        ROMFileExtensionsConfiguration IRomFileExtensionsConfigurationProvider.ROMFileExtensionsConfiguration => new();

        bool IDrawingSurfaceProvider.IsCreated => writeableBitmap != null;

        private WriteableBitmap writeableBitmap;

        public MainWindow()
        {
            InitializeComponent();

            (this as ILocalizationManager).SetLocale((this as ISettingsProvider).GetSetting<string>("Culture"));

            generalDependencyContainer = new GeneralDependencyContainer(
                dialogProvider: this,
                fileDialogProvider: this,
                recentRomsProvider: null,
                romFileExtensionsConfigurationProvider: this,
                savestateBoundsConfigurationProvider: this,
                themeManager: this,
                settingsManager: this,  
                localizationProvider: this,
                drawingSurfaceProvider: this,
                uIThreadDispatcherProvider: this);

            mainViewModel = new(generalDependencyContainer);

            DataContext = mainViewModel;

            (this as IThemeManager).SetTheme(Properties.Settings.Default.Theme);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        #region Interface Implementations 
        public (string ReturnedPath, bool Cancelled) OpenFileDialogPrompt(string[] validExtensions)
        {
            CommonOpenFileDialog dialog = new();
            string list = string.Empty;
            for (int i = 0; i < validExtensions.Length; i++)
            {
                list += $"*.{validExtensions[i]};";
            }

            dialog.Filters.Add(new((this as ILocalizationManager).GetString("SupportedFileFormats"), list));
            dialog.EnsureFileExists = dialog.EnsurePathExists = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            return result == CommonFileDialogResult.Ok ? ((string ReturnedPath, bool Cancelled))(dialog.FileName, false) : ((string ReturnedPath, bool Cancelled))(string.Empty, true);
        }
        public (string ReturnedPath, bool Cancelled) SaveFileDialogPrompt(string[] validExtensions)
        {
            CommonSaveFileDialog dialog = new();
            string list = string.Empty;
            for (int i = 0; i < validExtensions.Length; i++)
            {
                list += $"*.{validExtensions[i]};";
            }

            dialog.Filters.Add(new((this as ILocalizationManager).GetString("SupportedFileFormats"), list));
            CommonFileDialogResult result = dialog.ShowDialog();
            return result == CommonFileDialogResult.Ok ? ((string ReturnedPath, bool Cancelled))(dialog.FileName, false) : ((string ReturnedPath, bool Cancelled))(string.Empty, true);
        }

        public void ShowErrorDialog(string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _ = MessageBox.Show(message, (this as ILocalizationManager).GetString("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        string IThemeManager.GetTheme()
        {
            return ((ISettingsProvider)this).GetSetting<string>("Theme");
        }

        void IThemeManager.SetTheme(string themeName)
        {
            if (themeName.Equals("Light"))
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else if (themeName.Equals("Dark"))
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
            Properties.Settings.Default.Theme = themeName;
            Properties.Settings.Default.Save();
        }

        public void SetLocale(string localeKey)
        {
            (this as IUIThreadDispatcherProvider).Execute(delegate
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(localeKey);
                Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                LocalizationSource.Instance.CurrentCulture = culture;
                Properties.Settings.Default.Culture = localeKey;
                Properties.Settings.Default.Save();
            });
        }


        public T GetSetting<T>(string key)
        {
            return (T)Properties.Settings.Default[key];
        }
        public void SetSetting<T>(string key, T value)
        {
            Properties.Settings.Default[key] = value;
        }
        public void Save()
        {
            Properties.Settings.Default.Save();
        }
        public string GetString(string key)
        {
            return Properties.Resources.ResourceManager.GetString(key) ?? "?";
        }

        void IDrawingSurfaceProvider.Create(int width, int height)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                writeableBitmap = new(width, height, VisualTreeHelper.GetDpi(this).PixelsPerInchX, VisualTreeHelper.GetDpi(this).PixelsPerInchY, PixelFormats.Bgra32, null);
                Main_Image.Source = writeableBitmap;
            });
        }

        void IDrawingSurfaceProvider.Draw(Array buffer, int width, int height)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                writeableBitmap.WritePixels(new(0, 0, width, height), buffer, width * sizeof(int), 0);
            });
        }

        void IUIThreadDispatcherProvider.Execute(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }
        #endregion

        #region Windowing Commands

        [RelayCommand]
        private void ShowSettingsWindow()
        {
            // store reference and check against it being null because WPF accelerators are a dick and eat child window's events
            if (settingsWindow != null)
            {
                return;
            }
            settingsWindow = new SettingsWindow() { DataContext = new SettingsViewModel(generalDependencyContainer) };
            _ = settingsWindow.ShowDialog();
            settingsWindow = null; // this is okay, because ShowDialog() blocks
        }

        [RelayCommand]
        private void ShowROMInspectionWindow(object dataContext)
        {
            var romInspectionWindow = new ROMInspectionWindow() { DataContext = dataContext };
            romInspectionWindow.ShowDialog();
        }

        [RelayCommand]
        private void Exit()
        {
            Close();
        }

        #endregion

    }
}
