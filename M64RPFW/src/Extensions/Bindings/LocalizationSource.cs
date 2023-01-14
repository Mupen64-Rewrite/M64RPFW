using System.ComponentModel;
using System.Globalization;
using System.Resources;
using M64RPFW.Properties;

namespace M64RPFW.src.Extensions.Bindings;

// No ObservableObject. We must implement this manually to call invoke on PC event with instance as parameter, couldn't figure out how to do it with MVVM Toolkit.
public class LocalizationSource : INotifyPropertyChanged
{
    private readonly ResourceManager resManager = Resources.ResourceManager;
    private CultureInfo? currentCulture;
    public static LocalizationSource Instance { get; } = new();

    public string this[string key] => resManager.GetString(key, currentCulture);

    public CultureInfo CurrentCulture
    {
        get => currentCulture;
        set
        {
            if (currentCulture != value)
            {
                currentCulture = value;
                var @event = PropertyChanged;
                @event?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}