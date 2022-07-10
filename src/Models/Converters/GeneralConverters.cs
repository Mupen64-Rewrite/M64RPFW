using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace M64RPFW.UI.ViewModels.Converters
{
    public class StringEqualityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((string)(value)).Equals((string)parameter);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
    public class IntegerEqualityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (int)value == (int)parameter;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
    public class DoubleMercatorRoundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Math.Round((double)value, 0, MidpointRounding.ToEven);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
    public class DoubleMultiplierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (double)value * (double)parameter;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
    public class KeyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((Key)value).ToString();
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !((bool)value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !((bool)value);
    }
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
    public class IntegerToHexadecimalStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"0x{((int)value):X}";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
    public class UnsignedIntegerToHexadecimalStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => $"0x{((uint)value):X}";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

}
