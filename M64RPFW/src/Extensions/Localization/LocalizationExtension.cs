using System.Windows.Data;

namespace M64RPFW.UI.ViewModels.Extensions.Localization
{
    public class LocalizationExtension : Binding
    {
        public LocalizationExtension(string name) : base("[" + name + "]")
        {
            Mode = BindingMode.OneWay;
            Source = LocalizationSource.Instance;
        }
        public LocalizationExtension()
        {
            Mode = BindingMode.OneWay;
            Source = LocalizationSource.Instance;
        }
    }
}
