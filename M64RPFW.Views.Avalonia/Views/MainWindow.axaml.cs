using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Controls;
using M64RPFW.Views.Avalonia.Services;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        EmulatorViewModel = new EmulatorViewModel(this.Find<VidextControl>("EmulatorWindow")!, (App) Application.Current!,
            FilePickerService.Instance);
        SettingsViewModel = new SettingsViewModel(FilePickerService.Instance);

        DataContext = this;
        
        // ? vvv
        var mi = this.Find<MenuItem>("CurrentSlotMenu"); 
    }

    // avalonia compiled binding resolver lives in another assembly, so these have to be public :(
    public EmulatorViewModel EmulatorViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    private void Window_OnClosed(object? sender, EventArgs eventArgs)
    {
        EmulatorViewModel.OnWindowClosed();
    }

    private void ShowSettingsDialogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsDialog()
        {
            DataContext = SettingsViewModel
        };
        dialog.ShowDialog(this);
    }
}