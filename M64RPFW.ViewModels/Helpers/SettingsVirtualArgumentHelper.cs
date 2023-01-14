﻿namespace M64RPFW.ViewModels.Helpers;

public static class SettingsVirtualArgumentHelper
{
    /// <summary>
    ///     Splits and trims VArg into Key, Value components
    /// </summary>
    /// <param name="varg">Settings virtual argument in format "(Key, Value)"</param>
    /// <returns>Split Key, Value components</returns>
    public static string[] ParseVirtualArgument(string varg)
    {
        var args = varg.Replace("(", string.Empty).Replace(")", string.Empty).Split(',');
        for (var i = 0; i < args.Length; i++) args[i] = args[i].Trim();

        return args;
    }
}