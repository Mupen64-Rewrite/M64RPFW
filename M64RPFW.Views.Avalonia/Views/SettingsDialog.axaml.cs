using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace M64RPFW.Views.Avalonia.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = ((App)Application.Current!).ServiceProvider.GetService<SettingsViewModel>();
    }
}