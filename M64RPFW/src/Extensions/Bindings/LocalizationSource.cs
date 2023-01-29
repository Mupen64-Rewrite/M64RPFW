using System.ComponentModel;
using System.Globalization;
using System.Resources;
using M64RPFW.Properties;

namespace M64RPFW.Extensions.Bindings;

// No ObservableObject. We must implement this manually to call invoke on PC event with instance as parameter, couldn't figure out how to do it with MVVM Toolkit.
public class LocalizationSource : INotifyPropertyChanged
{
    private readonly ResourceManager _resManager = Resources.ResourceManager;
    private CultureInfo? _currentCulture;
    public static LocalizationSource Instance { get; } = new();

    public string this[string key] => _resManager.GetString(key, _currentCulture);

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            if (_currentCulture != value)
            {
                _currentCulture = value;
                var @event = PropertyChanged;
                @event?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}