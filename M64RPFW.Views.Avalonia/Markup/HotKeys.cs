using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.VisualTree;
using M64RPFW.Views.Avalonia.Helpers;

namespace M64RPFW.Views.Avalonia.Markup;

public class GlobalHotkeys : AvaloniaObject
{
    static GlobalHotkeys()
    {
        KeyStringProperty.Changed.ObserveChanges(args =>
        {
            var sender = (MenuItem) args.Sender;
            if (args.NewValue.HasValue)
            {
                var value = KeyGesture.Parse(args.NewValue.Value);
                sender.InputGesture = value;
                if (sender.GetValue(OwnedKeyBindingProperty) is { } keyBinding)
                {
                    keyBinding.Gesture = value;
                }
                else
                {
                    // TODO: if no root yet, wait until later to do this
                    var root = sender.GetVisualRoot()!;
                    var win = (InputElement) root;
                    keyBinding = new KeyBinding
                    {
                        Gesture = value,
                        [!KeyBinding.CommandProperty] = new Binding("Command", BindingMode.OneWay) { Source = sender },
                        [!KeyBinding.CommandParameterProperty] = new Binding("CommandParameter", BindingMode.OneWay)
                            { Source = sender }
                    };
                    win.KeyBindings.Add(keyBinding);
                }
            }
            else
            {
                if (sender.GetValue(OwnedKeyBindingProperty) is { } keyBinding)
                {
                    var root = (InputElement) sender.GetVisualRoot()!;
                    root.KeyBindings.Remove(keyBinding);
                    sender.SetValue(OwnedKeyBindingProperty, AvaloniaProperty.UnsetValue);
                }

                sender.InputGesture = null;
            }
        });
    }

    public static readonly AttachedProperty<string> KeyStringProperty =
        AvaloniaProperty.RegisterAttached<GlobalHotkeys, MenuItem, string>("KeyString");

    private static readonly AttachedProperty<KeyBinding?> OwnedKeyBindingProperty =
        AvaloniaProperty.RegisterAttached<GlobalHotkeys, MenuItem, KeyBinding?>("OwnedKeyGesture", null);


    public static string GetKeyString(MenuItem mi)
    {
        return mi.GetValue(KeyStringProperty);
    }

    public static void SetKeyString(MenuItem mi, string param)
    {
        mi.SetValue(KeyStringProperty, param);
    }
}