using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Utilities;

namespace M64RPFW.Views.Avalonia.Converters;

/// <summary>
/// An extension of <see cref="FuncValueConverter{TIn,TOut}"/> to include a ConvertBack function.
/// </summary>
/// <typeparam name="TIn"></typeparam>
/// <typeparam name="TOut"></typeparam>
public class TwoFuncValueConverter<TIn, TOut> : IValueConverter
{
    private readonly Func<TIn?, TOut> _convert;
    private readonly Func<TOut?, TIn> _convertBack;

    public TwoFuncValueConverter(Func<TIn?, TOut> convert, Func<TOut?, TIn> convertBack)
    {
        _convert = convert;
        _convertBack = convertBack;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return TypeUtilities.CanCast<TIn>(value) ? _convert((TIn?) value) : AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return TypeUtilities.CanCast<TOut>(value) ? _convertBack((TOut?) value) : AvaloniaProperty.UnsetValue;
    }
}