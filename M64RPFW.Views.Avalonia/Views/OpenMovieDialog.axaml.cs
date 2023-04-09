using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Views;

public partial class OpenMovieDialog : Window
{
    public OpenMovieDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        DataContext = new OpenMovieViewModel();
        ViewModel.OnCloseRequested += Close;
    }

    public OpenMovieViewModel ViewModel => (OpenMovieViewModel) DataContext!;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}