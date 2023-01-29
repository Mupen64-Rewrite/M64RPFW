using System.Windows;

namespace M64RPFW.Extensions.Bindings;

public class BindingProxy : Freezable
{
    // Using a DependencyProperty as the backing store for Data
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    #region Overrides of Freezable

    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }

    #endregion
}