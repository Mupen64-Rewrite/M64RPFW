using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using M64RPFW.Services;
using M64RPFW.ViewModels;
using Mupen64PlusRR.Controls;
using Mupen64PlusRR.Services;

namespace Mupen64PlusRR.Views;

public partial class MainWindow : Window, IDispatcherService
{

    public MainWindowViewModel MainWindowViewModel
    {
        set => DataContext = value;
        get => (MainWindowViewModel) DataContext!;
    }
    
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        MainWindowViewModel = new(this.Find<VidextControl>("EmulatorWindow"), this, new FilesService());
    }

    // public Task<string[]?> ShowOpenDialog(string title, List<FileDialogFilter> filters, bool allowMulti)
    // {
    //     OpenFileDialog ofd = new()
    //     {
    //         Title = title,
    //         Filters = filters,
    //         AllowMultiple = allowMulti
    //     };
    //     return ofd.ShowAsync(this);
    // }
    //
    // public Task<string?> ShowSaveDialog(string title, List<FileDialogFilter> filters)
    // {
    //     SaveFileDialog sfd = new()
    //     {
    //         Title = title,
    //         Filters = filters
    //     };
    //     return sfd.ShowAsync(this);
    // }
    //
    // public Task ShowSettingsDialog()
    // {
    //     var dialog = new SettingsDialog();
    //     return dialog.ShowDialog(this);
    // }
    public void Execute(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
}