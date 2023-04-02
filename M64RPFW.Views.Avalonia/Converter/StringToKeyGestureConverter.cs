using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace M64RPFW.Views.Avalonia.Converter;

public class StringToKeyGestureConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string @string)
        {
            return KeyGesture.Parse(@string);
        }
        else
        {
            throw new ArgumentException($"Expected {nameof(String)}, got {value?.GetType()}");
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is KeyGesture keyGesture)
        {
            return keyGesture.ToString();
        }
        else
        {
            throw new ArgumentException($"Expected {nameof(KeyGesture)}, got {value?.GetType()}");
        }
    }
}