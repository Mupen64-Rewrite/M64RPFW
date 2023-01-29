using System;
using System.Globalization;
using System.Windows.Data;

namespace M64RPFW.Converters;

public class IntegerEqualityToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)value == (int)parameter;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (int)parameter;
    }
}