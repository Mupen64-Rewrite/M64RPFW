using System;
using Avalonia;
using Avalonia.Controls;
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

    public AdvancedSettingsViewModel ViewModel => (AdvancedSettingsViewModel) DataContext!;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = new AdvancedSettingsViewModel();
    }

    private void TopLevel_OnClosed(object? sender, EventArgs e)
    {
        ViewModel.OnClosed();
    }
}