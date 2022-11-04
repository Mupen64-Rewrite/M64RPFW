﻿using M64RPFW.src.Views;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace M64RPFW.src.Converters
{
    public class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? MainWindow.LocalizationService.GetString("Yes") : MainWindow.LocalizationService.GetString("No");
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
