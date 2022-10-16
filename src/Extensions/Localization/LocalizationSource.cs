using System.ComponentModel;
using System.Globalization;

namespace M64RPFW.UI.ViewModels.Extensions.Localization
{
    // No ObservableObject. We must implement this manually to call invoke on PC event with instance as parameter, couldn't figure out how to do it with MVVM Toolkit.
    public class LocalizationSource : INotifyPropertyChanged
    {
        public static LocalizationSource Instance { get; } = new();

        private readonly System.Resources.ResourceManager resManager = Properties.Resources.ResourceManager;
        private CultureInfo currentCulture;

        public string this[string key] => resManager.GetString(key, currentCulture);

        public CultureInfo CurrentCulture
        {
            get => currentCulture;
            set
            {
                if (currentCulture != value)
                {
                    currentCulture = value;
                    PropertyChangedEventHandler @event = PropertyChanged;
                    @event?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
