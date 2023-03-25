using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Helpers;
using M64RPFW.Views.Avalonia.Services;
using M64RPFW.Views.Avalonia.Services.Abstractions;

namespace M64RPFW.Views.Avalonia.Controls;

public partial class FileBrowser : UserControl
{
    public FileBrowser()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> CurrentPathProperty =
        AvaloniaProperty.Register<FileBrowser, string>(nameof(CurrentPath), "");

    public string CurrentPath
    {
        get => GetValue(CurrentPathProperty);
        set => SetValue(CurrentPathProperty, value);
    }

    public static readonly StyledProperty<string> PickerTitleProperty =
        AvaloniaProperty.Register<FileBrowser, string>(nameof(PickerTitle), "Open file...");

    public string PickerTitle
    {
        get => GetValue(PickerTitleProperty);
        set => SetValue(PickerTitleProperty, value);
    }


    public static readonly StyledProperty<IEnumerable<FilePickerOption>?> PickerOptionsProperty =
        AvaloniaProperty.Register<FileBrowser, IEnumerable<FilePickerOption>?>(nameof(PickerOptions));

    public IEnumerable<FilePickerOption>? PickerOptions
    {
        get => GetValue(PickerOptionsProperty);
        set => SetValue(PickerOptionsProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var provider = WindowHelper.GetWindow().StorageProvider;
        var storageFiles = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = PickerTitle,
            AllowMultiple = false,
            FileTypeFilter = PickerOptions?.Select(fpo => fpo.ToFilePickerFileType()).ToArray()
        });
        if (storageFiles.Count == 0)
            return;

        CurrentPath = storageFiles[0].Path.LocalPath;
    }
}