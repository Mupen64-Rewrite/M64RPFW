using CommunityToolkit.Mvvm.Input;
using M64RPFW.Properties;
using M64RPFW.src.Configurations;
using M64RPFW.src.Containers;
using M64RPFW.src.Interfaces;
using M64RPFW.src.Themes;
using M64RPFW.UI.ViewModels;
using M64RPFW.UI.ViewModels.Extensions.Localization;
using M64RPFW.UI.Views;
using Microsoft.WindowsAPICodePack.Dialogs;
using ModernWpf;
using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace M64RPFW.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IFileDialogProvider, 
        IDialogProvider, 
        IRomFileExtensionsConfigurationProvider, 
        ISavestateBoundsConfigurationProvider, 
        IThemeManager, 
        IWindowClosingProvider, 
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

            (this as ILocalizationManager).SetLocale((this as ISettingsProvider).GetSettings().Culture);

            generalDependencyContainer = new GeneralDependencyContainer(
                this,
                this,
                null,
                this,
                this,
                this,
                this,
                this,
                this,
                this,
                this);

            mainViewModel = new(generalDependencyContainer);

            DataContext = mainViewModel;

            (this as IThemeManager).SetTheme(Properties.Settings.Default.Theme);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        public (string ReturnedPath, bool Cancelled) OpenFileDialogPrompt(string[] validExtensions)
        {
            CommonOpenFileDialog dialog = new();
            string list = string.Empty;
            for (int i = 0; i < validExtensions.Length; i++)
                list += $"*.{validExtensions[i]};";
            dialog.Filters.Add(new((this as ILocalizationManager).GetString("SupportedFileFormats"), list));
            dialog.EnsureFileExists = dialog.EnsurePathExists = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok) return (dialog.FileName, false);
            return (string.Empty, true);

        }
        public (string ReturnedPath, bool Cancelled) SaveFileDialogPrompt(string[] validExtensions)
        {
            CommonSaveFileDialog dialog = new();
            string list = string.Empty;
            for (int i = 0; i < validExtensions.Length; i++)
                list += $"*.{validExtensions[i]};";
            dialog.Filters.Add(new((this as ILocalizationManager).GetString("SupportedFileFormats"), list));
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok) return (dialog.FileName, false);
            return (string.Empty, true);

        }

        public void ShowErrorDialog(string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(message, (this as ILocalizationManager).GetString("Error"));
            }));
        }

        Themes IThemeManager.GetTheme()
        {
            return (Themes)Enum.Parse(typeof(Themes), (this as ISettingsProvider).GetSettings().Theme);
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
            CultureInfo culture = CultureInfo.GetCultureInfo(localeKey);
            Thread.CurrentThread.CurrentCulture =
            Thread.CurrentThread.CurrentUICulture =
            LocalizationSource.Instance.CurrentCulture = culture;
            Properties.Settings.Default.Culture = localeKey;
            Properties.Settings.Default.Save();
        }



        Settings ISettingsProvider.GetSettings()
        {
            return Properties.Settings.Default;
        }

        public string GetString(string key)
        {
            return Properties.Resources.ResourceManager.GetString(key) ?? "?";
        }

        [RelayCommand]
        private void ShowSettingsWindow()
        {
            if (settingsWindow != null)
            {
                return;
            }
            settingsWindow = new SettingsWindow() { DataContext = new SettingsViewModel(generalDependencyContainer) };
            settingsWindow.ShowDialog();
            settingsWindow = null; // this is okay, because ShowDialog() blocks
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
    }
}
