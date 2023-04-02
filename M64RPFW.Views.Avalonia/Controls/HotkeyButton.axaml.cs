using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<KeyGesture?> CurrentHotkeyProperty =
        AvaloniaProperty.Register<HotkeyButton, KeyGesture?>(nameof(CurrentHotkey),
            defaultBindingMode: BindingMode.TwoWay);

    public KeyGesture? CurrentHotkey
    {
        get => GetValue(CurrentHotkeyProperty);
        set => SetValue(CurrentHotkeyProperty, value);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // modifiers don't count
        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin)
            return;

        // Similar to Minecraft, disable shortcut on pressing Escape
        CurrentHotkey = e.Key == Key.Escape ? null : new KeyGesture(e.Key, e.KeyModifiers);

        // unfocus this element
        e.Device?.SetFocusedElement(null, NavigationMethod.Unspecified, KeyModifiers.None);
        wasActivated = false;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        wasActivated = true;
        this.Focus();
    }

    private bool wasActivated = false;
}