using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace M64RPFW.Views.Avalonia.Views;

public partial class ExceptionDialog : Window
{
    public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<ExceptionDialog, string>(
        nameof(Message), "Oh noes!!");

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly StyledProperty<string> DetailProperty = AvaloniaProperty.Register<ExceptionDialog, string>(
        "Exception");

    public string Detail
    {
        get => GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public ExceptionDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnOKClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}