using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Services;

namespace M64RPFW.Views.Avalonia.Views;

public partial class LuaWindow : Window
{
    // we need to correlate a lua window reference to a vm and frontend scripting service 
    public static Dictionary<LuaWindow, (LuaViewModel ViewModel, FrontendScriptingService FrontendScriptingService)> LuaViewModels = new();
    
    public LuaViewModel ViewModel => (LuaViewModel) DataContext!;
    
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
        LuaViewModels[this] = (new LuaViewModel(frontendScriptingService), frontendScriptingService);
        
        DataContext = LuaViewModels[this].ViewModel;
        Debug.Print(string.Join(", ", LuaViewModels.Select(pair => $"{pair.Key} => {pair.Value}")));
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        LuaViewModels.Remove(this);
    }

    public void Print(string value)
    {
        this.FindControl<TextBox>("LogTextBox").Text += $"{value}\r\n";
    }
}