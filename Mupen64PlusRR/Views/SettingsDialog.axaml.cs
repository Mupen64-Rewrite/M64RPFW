using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Mupen64PlusRR.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        AvaloniaXamlLoader.Load(this);
    }
}