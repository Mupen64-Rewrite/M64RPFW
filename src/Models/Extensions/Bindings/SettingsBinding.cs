using System.Windows.Data;

namespace M64RPFW.UI.ViewModels.Extensions.Bindings
{
    public class SettingsBindingExtension : Binding
    {
        public SettingsBindingExtension()
        {
            Initialize();
        }

        public SettingsBindingExtension(string path) : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = Properties.Settings.Default;
            this.Mode = BindingMode.OneWay;
        }
    }
}
