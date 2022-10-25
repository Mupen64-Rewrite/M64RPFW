using M64RPFW.src.Views;
using System.Windows.Data;

namespace M64RPFW.src.Extensions.Bindings
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
            Source = MainWindow.AppSettings;
            Mode = BindingMode.OneWay;
        }
    }
}
