using Avalonia.Input;

namespace M64RPFWAvalonia.src.Models.Helpers
{
    public static class WindowsTextHelper
    {
        public static string GetKeyVisual(Key key)
        {
            switch (key)
            {
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                    return key.ToString().Replace("NumPad", string.Empty);
                case Key.Multiply:
                    return "*";
                case Key.Add:
                    return "+";
                case Key.Subtract:
                    return "-";
                case Key.Decimal:
                    return ".";
                case Key.Divide:
                    return "/";
                case Key.OemSemicolon:
                    return ";";
                case Key.OemPlus:
                    return "+";
                case Key.OemComma:
                    return ",";
                case Key.OemMinus:
                    return "-";
                case Key.OemPeriod:
                    return ".";
                case Key.OemTilde:
                    return "`";
                case Key.OemOpenBrackets:
                    return "[";
                case Key.OemPipe:
                    return "|";
                case Key.OemCloseBrackets:
                    return "]";
                case Key.OemQuotes:
                    return "\"";
                case Key.OemBackslash:
                    return "\\";
                default:
                    {
                        if (key.ToString().Contains("Oem")) return "?";
                        else return key.ToString();
                    }
            }
        }

    }
}
