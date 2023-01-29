using System;
using System.Globalization;
using System.Windows.Data;

namespace M64RPFW.Converters;

public class DoubleMercatorRoundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Math.Round((double)value, 0, MidpointRounding.ToEven);
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}