using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace M64RPFW.Views.Avalonia.Controls;

/// <summary>
/// Custom <see cref="Decorator"/>-like class that can add/remove its child
/// from the visual tree without breaking the logical tree.
/// </summary>
public class VisualToggle : Control
{
    public static readonly StyledProperty<Control?> ChildProperty =
        AvaloniaProperty.Register<VisualToggle, Control?>(nameof(Child));

    public static readonly StyledProperty<bool> IsChildAttachedProperty = 
        AvaloniaProperty.Register<VisualToggle, bool>(nameof(IsChildAttached), defaultValue: false);

    static VisualToggle()
    {
        AffectsMeasure<VisualToggle>(ChildProperty);
        ChildProperty.Changed.AddClassHandler<VisualToggle, Control?>((x, e) => x.ChildChanged(e));
        IsChildAttachedProperty.Changed.AddClassHandler<VisualToggle, bool>((x, e) => x.IsChildAttachedChanged(e));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(Child, availableSize, new Thickness(0));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return LayoutHelper.ArrangeChild(Child, finalSize, new Thickness(0));
    }

    [Content]
    public Control? Child
    {
        get => GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }

    public bool IsChildAttached
    {
        get => GetValue(IsChildAttachedProperty);
        set => SetValue(IsChildAttachedProperty, value);
    }

    private void ChildChanged(AvaloniaPropertyChangedEventArgs<Control?> e)
    {
        var oldChild = e.OldValue.GetValueOrDefault();
        var newChild = e.NewValue.GetValueOrDefault();
        
        if (oldChild != null)
        {
            ((ISetLogicalParent)oldChild).SetParent(null);
            LogicalChildren.Clear();
            VisualChildren.Remove(oldChild);
        }

        if (newChild != null)
        {
            ((ISetLogicalParent)newChild).SetParent(this);
            if (IsChildAttached)
                VisualChildren.Add(newChild);
            LogicalChildren.Add(newChild);
        }
    }

    private void IsChildAttachedChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        bool oldValue = e.OldValue.Value;
        bool newValue = e.NewValue.Value;

        if (Child == null)
            return;

        if (!oldValue && newValue)
        {
            VisualChildren.Add(Child);
        }
        else if (oldValue && !newValue)
        {
            VisualChildren.Remove(Child);
        }
    }
}