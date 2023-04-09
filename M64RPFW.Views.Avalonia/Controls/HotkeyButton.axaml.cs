using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace M64RPFW.Views.Avalonia.Controls;

public partial class HotkeyButton : UserControl
{
    public HotkeyButton()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<KeyGesture> CurrentHotkeyProperty =
        AvaloniaProperty.Register<HotkeyButton, KeyGesture>(nameof(CurrentHotkey),
            defaultBindingMode: BindingMode.TwoWay);

    public KeyGesture CurrentHotkey
    {
        get => GetValue(CurrentHotkeyProperty);
        set => SetValue(CurrentHotkeyProperty, value);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // Button has to have been activated for this to work
        if (Button?.IsChecked is null or false)
            return;
        // modifiers don't count
        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin)
            return;

        // Similar to Minecraft, disable shortcut on pressing Escape
        CurrentHotkey = e.Key == Key.Escape ? new KeyGesture(Key.None) : new KeyGesture(e.Key, e.KeyModifiers);

        // unfocus this element
        e.Device?.SetFocusedElement(null, NavigationMethod.Unspecified, KeyModifiers.None);
        Button.IsChecked = false;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Focus();
    }
}