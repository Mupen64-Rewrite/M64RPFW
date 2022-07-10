using M64RPFW.UI.ViewModels;
using M64RPFW.UI.ViewModels.Helpers;
using M64RPFW.UI.ViewModels.Interaction;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Threading;
using System.Windows;

namespace M64RPFW.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public readonly MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new();
            this.DataContext = mainViewModel;

            DialogHelper.CurrentSnackbar = Main_Snackbar;
            ThemeHelper.SetTheme(Properties.Settings.Default.Theme);

            Main_OpenGLControl.Start(new() { MajorVersion = 4, MinorVersion = 2 });

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Main_OpenGLControl_Render(TimeSpan obj)
        {
            GL.ClearColor(1f, 1f, 1f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
