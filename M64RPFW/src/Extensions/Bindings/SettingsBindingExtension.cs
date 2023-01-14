using System.Windows.Data;
using M64RPFW.src.Views;

namespace M64RPFW.src.Extensions.Bindings;

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