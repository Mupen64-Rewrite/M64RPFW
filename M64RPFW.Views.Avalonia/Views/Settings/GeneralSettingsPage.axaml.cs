using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace M64RPFW.Views.Avalonia.Views.Settings;

public partial class GeneralSettingsPage : UserControl
{
    public GeneralSettingsPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}