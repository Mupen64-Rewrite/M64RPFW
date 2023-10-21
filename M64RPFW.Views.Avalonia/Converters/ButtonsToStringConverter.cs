using System;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;
using M64RPFW.Models.Types;

namespace M64RPFW.Views.Avalonia.Converters;

public class ButtonsToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Mupen64PlusTypes.Buttons buttons)
            throw new ArgumentException($"Expected {nameof(Mupen64PlusTypes.Buttons)}, got {value?.GetType().FullName}");
        var result = new StringBuilder($"({buttons.JoyX}, {buttons.JoyY}) ", 50);

        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.A))
            result.Append("A ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.B))
            result.Append("B ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.Start))
            result.Append("S ");
        
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.Z))
            result.Append("Z ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.L))
            result.Append("L ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.R))
            result.Append("R ");
        
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CLeft))
            result.Append("C\u25C2 ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CUp))
            result.Append("C\u25B4 ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CRight))
            result.Append("C\u25B8 ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CDown))
            result.Append("C\u25BE ");
        
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DLeft))
            result.Append("D\u25C2 ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DUp))
            result.Append("D\u25B4 ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DRight))
            result.Append("D\u25B8 ");
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DDown))
            result.Append("D\u25BE ");

        return result.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}