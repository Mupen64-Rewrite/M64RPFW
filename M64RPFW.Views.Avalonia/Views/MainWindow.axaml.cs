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

        DataContext = new EmulatorViewModel(this.Find<VidextControl>("EmulatorWindow")!, (App) Application.Current!,
            FilePickerService.Instance);

        var mi = this.Find<MenuItem>("CurrentSlotMenu");
    }

    private EmulatorViewModel ViewModel => (EmulatorViewModel) DataContext!;

    private void Window_OnClosed(object? sender, EventArgs eventArgs)
    {
        ViewModel.OnWindowClosed();
    }

    private void ShowSettingsDialogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsDialog();
        dialog.ShowDialog(this);
    }
}