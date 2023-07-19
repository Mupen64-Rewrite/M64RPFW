using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace M64RPFW.Views.Avalonia.Views.Settings;

public partial class EmulatorSettingsPage : UserControl
{
    public EmulatorSettingsPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}