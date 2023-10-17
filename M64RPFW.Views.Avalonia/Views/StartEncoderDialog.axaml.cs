using Avalonia;
using Avalonia.Controls;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Views;

public partial class StartEncoderDialog : Window
{
    public StartEncoderDialog()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        DataContext = new StartEncoderViewModel();
        ViewModel.OnCloseRequested += Close;
    }

    public StartEncoderViewModel ViewModel => (StartEncoderViewModel) DataContext!;
    
}