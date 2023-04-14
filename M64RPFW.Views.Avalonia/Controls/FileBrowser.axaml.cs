using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Helpers;
using M64RPFW.Views.Avalonia.Services.Abstractions;

namespace M64RPFW.Views.Avalonia.Controls;

public partial class FileBrowser : UserControl
{
    public FileBrowser()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<bool> IsOpenDialogProperty =
        AvaloniaProperty.Register<FileBrowser, bool>(nameof(IsOpenDialog), defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsOpenDialog
    {
        get => GetValue(IsOpenDialogProperty);
        set => SetValue(IsOpenDialogProperty, value);
    }

    
    public static readonly StyledProperty<string> CurrentPathProperty =
        AvaloniaProperty.Register<FileBrowser, string>(nameof(CurrentPath), defaultValue: "",
            defaultBindingMode: BindingMode.TwoWay);

    public string CurrentPath
    {
        get => GetValue(CurrentPathProperty);
        set
        {
            if (IsOpenDialog && !File.Exists(value))
                throw new ArgumentException("Provided path is invalid");
            SetValue(CurrentPathProperty, value);
        }
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

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<FileBrowser, string>(nameof(Watermark), "Path");

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
       
        
        var provider = WindowHelper.GetWindow().StorageProvider;
        
        IStorageFolder? suggestedStartLocation;

        try
        {
            // try to get a known path first. if this fails, fall back to a known location. 
            suggestedStartLocation = await provider.TryGetFolderFromPathAsync(Path.GetDirectoryName(CurrentPath)!);
        }
        catch
        {
            // shouldn't throw... hopefully :P
            suggestedStartLocation = await provider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
        }
        
        IStorageFile? storageFile = null;

        if (IsOpenDialog)
        {
            var storageFiles = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = PickerTitle,
                AllowMultiple = false,
                FileTypeFilter = PickerOptions?.Select(fpo => fpo.ToFilePickerFileType()).ToArray(),
                SuggestedStartLocation = suggestedStartLocation
            });
            if (storageFiles.Count > 0)
            {
                storageFile = storageFiles[0];
            }
        }
        else
        {

            storageFile = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = PickerTitle,
                SuggestedStartLocation = suggestedStartLocation
            });

        }
        
        if (storageFile == null)
            return;

        CurrentPath = storageFile.Path.LocalPath;
    }
}