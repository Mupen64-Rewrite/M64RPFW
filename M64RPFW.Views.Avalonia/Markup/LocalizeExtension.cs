using System;
using Avalonia;
using Avalonia.Data;

namespace M64RPFW.Views.Avalonia.Markup;

public class LocalizeExtension : IBinding
{
    public LocalizeExtension(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public IBinding ProvideValue(IServiceProvider sp)
    {
        return this;
    }
    
    public InstancedBinding? Initiate(AvaloniaObject target, AvaloniaProperty? targetProperty, object? anchor = null,
        bool enableDataValidation = false)
    {
        return InstancedBinding.OneWay(LocalizationSource.Instance.GetObservable(Path));
    }
}