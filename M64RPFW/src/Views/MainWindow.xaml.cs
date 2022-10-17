using M64RPFW.src.Configurations;
using M64RPFW.src.Containers;
using M64RPFW.src.Interfaces;
using M64RPFW.src.Themes;
using M64RPFW.UI.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Threading;
using System.Windows;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace M64RPFW.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IFileDialogProvider, IDialogProvider, IRomFileExtensionsConfigurationProvider, ISavestateBoundsConfigurationProvider, IThemeManager, IWindowClosingProvider
    {
        private readonly MainViewModel mainViewModel;
        private readonly GeneralDependencyContainer generalDependencyContainer;

        SavestateBoundsConfiguration ISavestateBoundsConfigurationProvider.SavestateBoundsConfiguration => new();

        ROMFileExtensionsConfiguration IRomFileExtensionsConfigurationProvider.ROMFileExtensionsConfiguration => new();


        public MainWindow()
        {
            InitializeComponent();

            generalDependencyContainer = new GeneralDependencyContainer(
                this,
                this,
                null,
                this,
                this,
                this,
                this);

            mainViewModel = new(generalDependencyContainer);

            DataContext = mainViewModel;

            (this as IThemeManager).SetTheme(Properties.Settings.Default.Theme);

            Main_OpenGLControl.Start(new() { MajorVersion = 4, MinorVersion = 2 });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Main_OpenGLControl_Render(TimeSpan obj)
        {
            GL.ClearColor(1f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public (string ReturnedPath, bool Cancelled) OpenFileDialogPrompt(string[] validExtensions)
        {
            CommonOpenFileDialog dialog = new();
            string list = string.Empty;
            for (int i = 0; i < validExtensions.Length; i++)
                list += $"*.{validExtensions[i]};";
            dialog.Filters.Add(new("Supported files", list));
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
            dialog.Filters.Add(new("Supported files", list));
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok) return (dialog.FileName, false);
            return (string.Empty, true);

        }

        public void ShowErrorDialog(string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(message, Properties.Resources.Error);
            }));
        }

        Themes IThemeManager.GetTheme()
        {
            return (Themes)Enum.Parse(typeof(Themes), Properties.Settings.Default.Theme);
        }

        void IThemeManager.SetTheme(string themeName)
        {
            Properties.Settings.Default.Theme = themeName;
            Properties.Settings.Default.Save();
        }
    }
}
