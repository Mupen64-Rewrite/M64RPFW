using System.Windows.Data;

namespace M64RPFW.src.Extensions.Bindings
{
    public class LocalizationBindingExtension : Binding
    {
        public LocalizationBindingExtension(string name) : base("[" + name + "]")
        {
            Initialize();
        }

        private void Initialize()
        {
            Mode = BindingMode.OneWay;
            Source = LocalizationSource.Instance;
        }

        public LocalizationBindingExtension()
        {
            Mode = BindingMode.OneWay;
            Source = LocalizationSource.Instance;
        }
    }
}
