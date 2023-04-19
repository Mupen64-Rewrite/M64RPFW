using System;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;

namespace M64RPFW.Views.Avalonia.MarkupExtensions;

public class LocaleString : MarkupExtension
{
    public LocaleString(string path)
    {
        Path = path;
    }

    public string Path { get; set; }
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new ReflectionBindingExtension
        {
            Path = $"[{Path}]",
            Source = LocalizationSource.Instance
        }.ProvideValue(serviceProvider);
    }
}