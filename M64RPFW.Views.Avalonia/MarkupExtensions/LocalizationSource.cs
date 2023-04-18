using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace M64RPFW.Views.Avalonia.MarkupExtensions;

public class LocalizationSource : INotifyPropertyChanged
{
    public static LocalizationSource Instance { get; } = new();

    private readonly ResourceManager _resourceManager = Resources.Resources.ResourceManager;
    private CultureInfo _currentCulture = null!;
    public string this[string key] => _resourceManager.GetString(key, _currentCulture) ?? $"[{key}]";

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                    _currentCulture = value;
            var @event = PropertyChanged;
            @event?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;
}