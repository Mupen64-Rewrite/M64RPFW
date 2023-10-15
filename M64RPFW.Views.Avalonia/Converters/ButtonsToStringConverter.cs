using System;
using System.Globalization;
using Avalonia.Data.Converters;
using M64RPFW.Models.Types;

namespace M64RPFW.Views.Avalonia.Converters;

public class ButtonsToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Mupen64PlusTypes.Buttons buttons)
            throw new ArgumentException($"Expected {nameof(Mupen64PlusTypes.Buttons)}, got {value?.GetType().FullName}");
        var result = $"({buttons.JoyX}, {buttons.JoyY}) ";

        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.A))
        {
            result += "A ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.B))
        {
            result += "B ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.Start))
        {
            result += "S ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.Z))
        {
            result += "Z ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.L))
        {
            result += "L ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.R))
        {
            result += "R ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CLeft))
        {
            result += "C< ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CUp))
        {
            result += "C^ ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CRight))
        {
            result += "C> ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.CDown))
        {
            result += "Cv ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DLeft))
        {
            result += "D< ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DUp))
        {
            result += "D^ ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DRight))
        {
            result += "D> ";
        }
        if (buttons.BtnMask.HasFlag(Mupen64PlusTypes.ButtonMask.DDown))
        {
            result += "Dv ";
        }
        
        return result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}