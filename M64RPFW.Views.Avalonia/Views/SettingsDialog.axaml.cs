using System;
using System.Globalization;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.MarkupExtensions;

namespace M64RPFW.Views.Avalonia.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = SettingsViewModel.Instance;
    }

    private void TopLevel_OnClosed(object? sender, EventArgs e)
    {
        ((SettingsViewModel)DataContext!).SaveCommand.Execute(null);
    }
}