using Avalonia.Data;

namespace M64RPFWAvalonia.UI.ViewModels.Extensions.Localization
{
    public class LocalizationExtension : Binding
    {
        public LocalizationExtension(string name) : base("[" + name + "]")
        {
            this.Mode = BindingMode.OneWay;
            this.Source = LocalizationSource.Instance;
        }
        public LocalizationExtension()
        {
            this.Mode = BindingMode.OneWay;
            this.Source = LocalizationSource.Instance;
        }
    }
}
