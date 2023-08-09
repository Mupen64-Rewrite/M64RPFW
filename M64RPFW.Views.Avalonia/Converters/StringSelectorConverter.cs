using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data.Converters;

namespace M64RPFW.Views.Avalonia.Converters;

/// <summary>
/// Value converter converting a boolean to either one of two strings.
///
/// Formats are specified as <code>value if true\value if false</code>. You may use <code>\\</code>
/// to escape a backslash.
/// </summary>
public class StringSelectorConverter : IValueConverter
{
    private static readonly Regex SpecSplitter = new(@"(?<!\\)\\(?!\\)");
    private static readonly Regex DeEscaper = new(@"\\{2}");
    
    private static string[] ParseSpec(string param)
    {
        string[] parts = SpecSplitter.Split(param);
        if (parts.Length != 2)
            throw new ArgumentException("Parameter does not match conversion spec.");
        parts[0] = DeEscaper.Replace(parts[0], "\\");
        parts[1] = DeEscaper.Replace(parts[1], "\\");
        return parts;
    }
    
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool @bool)
            throw new ArgumentException($"Expected bool, got {value?.GetType().FullName}");
        if (parameter is not string spec)
            throw new ArgumentException("Expected");
        var parsed = ParseSpec(spec);

        return @bool ? parsed[0] : parsed[1];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}