using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace M64RPFW.Converters;

public class KeyBindingToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var kb = (KeyBinding)value;
        return kb.Modifiers == ModifierKeys.None ? $"{kb.Key}" : $"{kb.Modifiers}+{kb.Key}";
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}