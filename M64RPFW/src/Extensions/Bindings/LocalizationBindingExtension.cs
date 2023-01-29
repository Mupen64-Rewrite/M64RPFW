using System.Windows.Data;

namespace M64RPFW.Extensions.Bindings;

public class LocalizationBindingExtension : Binding
{
    public LocalizationBindingExtension(string name) : base("[" + name + "]")
    {
        Initialize();
    }

    public LocalizationBindingExtension()
    {
        Mode = BindingMode.OneWay;
        Source = LocalizationSource.Instance;
    }

    private void Initialize()
    {
        Mode = BindingMode.OneWay;
        Source = LocalizationSource.Instance;
    }
}