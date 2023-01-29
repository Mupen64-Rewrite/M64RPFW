using System;
using System.Globalization;
using System.Windows.Data;
using M64RPFW.Views;

namespace M64RPFW.Converters;

public class BooleanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value
            ? MainWindow.LocalizationService.GetStringOrDefault("Yes")
            : MainWindow.LocalizationService.GetStringOrDefault("No");
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}