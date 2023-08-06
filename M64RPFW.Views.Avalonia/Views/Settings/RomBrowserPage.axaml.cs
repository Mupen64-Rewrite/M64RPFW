using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.ViewModels;
using M64RPFW.ViewModels.Extensions;
using M64RPFW.Views.Avalonia.Helpers;
using M64RPFW.Views.Avalonia.Services;

namespace M64RPFW.Views.Avalonia.Views.Settings;

public partial class RomBrowserPage : UserControl
{
    // FIXME: avalonia's control reference sourcegen doesn't work (always produces null references)
    private ListBox PathListBox => this.Get<ListBox>("ListBox");
    
    public RomBrowserPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    // TODO: lift this into a generic editable listbox uc
    private async void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var folders = await FilePickerService.Instance.ShowOpenFolderPickerAsync(allowMultiple: true);
    
        if (folders != null)
        {
            SettingsViewModel.Instance.RomBrowserPaths.AddRange(folders);
        }
    }
    
    private void RemoveSelectedButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (PathListBox.SelectedIndex >= 0 && PathListBox.SelectedIndex < SettingsViewModel.Instance.RomBrowserPaths.Count)
        {
            SettingsViewModel.Instance.RomBrowserPaths.RemoveAt(PathListBox.SelectedIndex);
        }
    }
}