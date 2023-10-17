using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using M64RPFW.ViewModels;
using M64RPFW.ViewModels.Extensions;

namespace M64RPFW.Views.Avalonia.Controls;

public partial class RomBrowserControl : UserControl
{
    public RomBrowserViewModel RomBrowserViewModel { get; } = new();
    public EmulatorViewModel? EmulatorViewModel
    {
        get => EmulatorViewModel.Instance;
    }

    private bool _isRefreshing;

    public static readonly DirectProperty<RomBrowserControl, bool> IsRefreshingProperty = AvaloniaProperty.RegisterDirect<RomBrowserControl, bool>(
        "IsRefreshing", o => o.IsRefreshing, (o, v) => o.IsRefreshing = v);

    public bool IsRefreshing
    {
        get => _isRefreshing;
        private set => SetAndRaise(IsRefreshingProperty, ref _isRefreshing, value);
    }

    public RomBrowserControl()
    {
        InitializeComponent();
        DataContext = this;

        RomBrowserViewModel.PropertyChanged += (_, _) =>
        {
            IsRefreshing = RomBrowserViewModel.IsRefreshing;
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }


    private void OpenSelectedRom(RomBrowserItemViewModel romBrowserItemViewModel)
    {
        EmulatorViewModel!.OpenRomFromPathCommand.ExecuteIfPossible(commandParam: romBrowserItemViewModel.Path);
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