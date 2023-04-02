using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace M64RPFW.Views.Avalonia.Converter;

public class DefaultValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() ?? parameter;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}