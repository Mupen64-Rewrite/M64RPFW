using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Extensions;
using M64RPFW.Views.Avalonia.Helpers;
using M64RPFW.Views.Avalonia.Services;

namespace M64RPFW.Views.Avalonia.Views;

public partial class LuaWindow : Window
{

    public LuaViewModel ViewModel => (LuaViewModel)DataContext!;
    public LuaWindowService ScriptingService { get; }

    public LuaWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        ScriptingService = new LuaWindowService(this);
        DataContext = new LuaViewModel(ScriptingService, WindowHelper.MainWindow, FilePickerService.Instance);
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        ViewModel.StopCommand.ExecuteIfPossible();
    }

    public void Print(string value) 
    {
        Dispatcher.UIThread.Post(() => {
            LogTextBox.Text += $"{value}\r\n";
            LogScrollViewer.ScrollToEnd();
        }, DispatcherPriority.Background);
    }

    private void OnClearClicked(object? sender, RoutedEventArgs e)
    {
        LogTextBox.Text = "";
    }
}