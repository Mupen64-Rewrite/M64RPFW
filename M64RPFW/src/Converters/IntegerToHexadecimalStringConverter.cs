using System;
using System.Globalization;
using System.Windows.Data;

namespace M64RPFW.src.Converters;

public class IntegerToHexadecimalStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return $"0x{(int)value:X}";
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}