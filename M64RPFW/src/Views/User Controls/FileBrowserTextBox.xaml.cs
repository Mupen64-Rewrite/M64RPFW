using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace M64RPFW.src.Views.User_Controls;

/// <summary>
///     Interaction logic for FileBrowserTextBox.xaml
/// </summary>
public partial class FileBrowserTextBox : UserControl
{
    public static readonly DependencyProperty PathProperty =
        DependencyProperty.Register("Path", typeof(string), typeof(FileBrowserTextBox), new PropertyMetadata(default));

    public static readonly DependencyProperty BrowseCommandProperty =
        DependencyProperty.Register("BrowseCommand", typeof(ICommand), typeof(FileBrowserTextBox),
            new PropertyMetadata(default));


    public FileBrowserTextBox()
    {
        InitializeComponent();
    }


    public string Path
    {
        get => (string)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public ICommand BrowseCommand
    {
        get => (ICommand)GetValue(BrowseCommandProperty);
        set => SetValue(BrowseCommandProperty, value);
    }
}