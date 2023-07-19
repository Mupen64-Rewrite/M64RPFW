using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Extensions;
using M64RPFW.Views.Avalonia.Helpers;
using M64RPFW.Views.Avalonia.Services;

namespace M64RPFW.Views.Avalonia.Views;

public partial class LuaWindow : Window
{
    // we need to correlate a lua window reference to a vm and frontend scripting service 
    public static Dictionary<LuaWindow, (LuaViewModel ViewModel, FrontendScriptingService FrontendScriptingService)>
        LuaViewModels = new();

    public LuaViewModel ViewModel => (LuaViewModel)DataContext!;
    private TextBox LoggingTextBox => this.FindControl<TextBox>("LogTextBox")!;
    
    public LuaWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        var frontendScriptingService = new FrontendScriptingService(this);
        LuaViewModels[this] = (new LuaViewModel(frontendScriptingService),
            frontendScriptingService);

        DataContext = LuaViewModels[this].ViewModel;
        Debug.Print(string.Join(", ", LuaViewModels.Select(pair => $"{pair.Key} => {pair.Value}")));
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        ViewModel.StopCommand.ExecuteIfPossible();
        LuaViewModels.Remove(this);
    }

    public void Print(string value)
    {
        Dispatcher.UIThread.Post(() => { LoggingTextBox.Text += $"{value}\r\n"; });
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        LoggingTextBox.Text = "";
    }
}