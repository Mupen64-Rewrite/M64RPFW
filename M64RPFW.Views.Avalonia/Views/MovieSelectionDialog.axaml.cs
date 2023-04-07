using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.ViewModels;

namespace M64RPFW.Views.Avalonia.Views;

public partial class MovieSelectionDialog : Window
{
    public MovieSelectionViewModel MovieSelectionViewModel { get; } = new();
    
    public MovieSelectionDialog()
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
}