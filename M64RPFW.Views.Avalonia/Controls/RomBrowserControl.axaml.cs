using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Extensions;

namespace M64RPFW.Views.Avalonia.Controls;

public partial class RomBrowserControl : UserControl
{
    public RomBrowserViewModel RomBrowserViewModel { get; } = new();
    public EmulatorViewModel? EmulatorViewModel
    {
        get => EmulatorViewModel.Instance;
    }

    public RomBrowserControl()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }


    private void OpenSelectedRom(RomBrowserItemViewModel romBrowserItemViewModel)
    {
        EmulatorViewModel.Instance.OpenRomFromPathCommand.ExecuteIfPossible(executeParameter: romBrowserItemViewModel.Path);
    }

    private void DataGrid_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as DataGrid)?.SelectedItem is RomBrowserItemViewModel romBrowserItemViewModel)
        {
            OpenSelectedRom(romBrowserItemViewModel);
        }
    }

    private void DataGrid_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }
        if ((sender as DataGrid)?.SelectedItem is RomBrowserItemViewModel romBrowserItemViewModel)
        {
            OpenSelectedRom(romBrowserItemViewModel);
        }
    }
}