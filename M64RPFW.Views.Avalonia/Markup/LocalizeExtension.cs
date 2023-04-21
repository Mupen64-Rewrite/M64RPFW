using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace M64RPFW.Views.Avalonia.Markup;

/// <summary>
/// Binds to a string in localization data.
/// </summary>
public class LocalizeExtension
{
    /// <summary>
    /// Binds to the localization entry with the specified key.
    /// </summary>
    /// <param name="key">the localization key.</param>
    public LocalizeExtension(string key)
    {
        Key = key;
    }

    public string Key { get; }

    // ReSharper disable once UnusedMember.Global
    public IBinding ProvideValue(IServiceProvider sp)
    {
        ReflectionBindingExtension rbe = new($"[{Key}]")
        {
            Source = LocalizationSource.Instance
        };
        return rbe.ProvideValue(sp);
    }
}