using System.Windows;
using System.Windows.Controls;

namespace M64RPFW.Views.User_Controls;

/// <summary>
///     Interaction logic for SettingsSection.xaml
/// </summary>
public partial class SettingsSection : UserControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(SettingsSection), new PropertyMetadata(null));

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register("Content", typeof(object), typeof(SettingsSection), new PropertyMetadata(null));


    public SettingsSection()
    {
        InitializeComponent();
    }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }


    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}