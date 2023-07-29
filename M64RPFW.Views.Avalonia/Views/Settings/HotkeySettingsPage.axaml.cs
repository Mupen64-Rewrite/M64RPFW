using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace M64RPFW.Views.Avalonia.Views.Settings;

public partial class HotkeySettingsPage : UserControl
{
    public HotkeySettingsPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}