using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using M64RPFW.ViewModels;
using M64RPFW.Views.Avalonia.Converters;

namespace M64RPFW.Views.Avalonia.Markup;

/// <summary>
/// Markup extension providing access to the settings thing.
/// </summary>
public class SettingExtension
{
    private static readonly StringToKeyGestureConverter Converter = new();
    
    public SettingExtension(string path)
    {
        Path = path;
    }
    
    public string Path { get; set; }

    public IBinding ProvideValue(IServiceProvider sp)
    {
        ReflectionBindingExtension rbe = new ReflectionBindingExtension(Path)
        {
            Mode = BindingMode.OneWay,
            Source = SettingsViewModel.Instance,
            Converter = Converter
        };
        return rbe.ProvideValue(sp);
    }
}