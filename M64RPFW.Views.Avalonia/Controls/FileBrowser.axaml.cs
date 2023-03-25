using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using M64RPFW.Services.Abstractions;

namespace M64RPFW.Views.Avalonia.Controls;

public partial class FileBrowser : UserControl
{
    public FileBrowser()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> CurrentPathProperty =
        AvaloniaProperty.Register<FileBrowser, string>(nameof(CurrentPath));

    public string CurrentPath
    {
        get => GetValue(CurrentPathProperty);
        set => SetValue(CurrentPathProperty, value);
    }

    public static readonly StyledProperty<IEnumerable<FilePickerOption>> PickerOptionsProperty =
        AvaloniaProperty.Register<FileBrowser, IEnumerable<FilePickerOption>>(nameof(PickerOptions));

    public IEnumerable<FilePickerOption> PickerOptions
    {
        get => GetValue(PickerOptionsProperty);
        set => SetValue(PickerOptionsProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("CLICK");
    }
}