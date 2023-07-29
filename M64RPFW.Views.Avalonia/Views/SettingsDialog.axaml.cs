using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = SettingsViewModel.Instance;
    }

    public SettingsViewModel ViewModel => (SettingsViewModel) DataContext!;

    private void OnApplyClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel.SaveCommand.Execute(null);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnOKClicked(object? sender, RoutedEventArgs e)
    {
        ViewModel.SaveCommand.Execute(null);
        Close();
    }
}