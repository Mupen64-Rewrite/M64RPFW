using M64RPFWAvalonia.Properties;
using System.ComponentModel;
using System.Globalization;

namespace M64RPFWAvalonia.UI.ViewModels.Extensions.Localization
{
    // No ObservableObject. We must implement this manually to call invoke on PC event with instance as parameter, couldn't figure out how to do it with MVVM Toolkit.
    public class LocalizationSource : INotifyPropertyChanged
    {
        public static LocalizationSource Instance { get; } = new();

        private readonly System.Resources.ResourceManager resManager = Resources.ResourceManager;
        private CultureInfo currentCulture;

        public string this[string key] => this.resManager.GetString(key, this.currentCulture);

        public CultureInfo CurrentCulture
        {
            get => this.currentCulture;
            set
            {
                if (this.currentCulture != value)
                {
                    this.currentCulture = value;
                    PropertyChangedEventHandler @event = this.PropertyChanged;
                    @event?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
