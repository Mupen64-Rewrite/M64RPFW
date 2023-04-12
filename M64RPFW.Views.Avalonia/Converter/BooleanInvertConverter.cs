using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace M64RPFW.Views.Avalonia.Converter;

public class BooleanInvertConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool @bool)
        {
            return !@bool;
        }

        throw new ArgumentException($"Value was not of type {nameof(Boolean)}");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool @bool)
        {
            return !@bool;
        }

        throw new ArgumentException($"Value was not of type {nameof(Boolean)}");
    }
}