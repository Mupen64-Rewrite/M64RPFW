using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Views;

public partial class AdvancedSettingsDialog : Window
{
    public AdvancedSettingsDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private bool _didSave = false;

    public AdvancedSettingsViewModel ViewModel => (AdvancedSettingsViewModel) DataContext!;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = new AdvancedSettingsViewModel();
    }

    private void TopLevel_OnClosed(object? sender, EventArgs e)
    {
        if (!_didSave)
            ViewModel.Revert();
    }

    private void OnApplyClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel.Save();
        SettingsViewModel.Instance.NotifyAllPropertiesChanged();
        _didSave = true;
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnOKClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel.Save();
        SettingsViewModel.Instance.NotifyAllPropertiesChanged();
        _didSave = true;
        Close();
    }
}