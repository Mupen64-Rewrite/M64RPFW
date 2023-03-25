using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Services;

namespace M64RPFW.Views.Avalonia.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = new SettingsViewModel(FilePickerService.Instance);
    }

    private SettingsViewModel ViewModel => (SettingsViewModel) DataContext!;
    
    private void TopLevel_OnClosed(object? sender, EventArgs e)
    {
        ViewModel.OnClosed();
    }
}