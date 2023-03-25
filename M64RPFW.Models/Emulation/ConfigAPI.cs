using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.Models.Emulation;

public static partial class Mupen64Plus
{
    public static IEnumerable<string> ListConfigSections()
    {
        
        
        List<string> res = new();
        Mupen64PlusTypes.Error err = _fnConfigListSections(IntPtr.Zero, (_, name) => res.Add(name));
        ThrowForError(err);
        
        return res;
    }

    public static IntPtr ConfigOpenSection(string name)
    {
        Mupen64PlusTypes.Error err = _fnConfigOpenSection(name, out IntPtr handle);
        ThrowForError(err);

        return handle;
    }

    public static IEnumerable<(string name, Type type)> ConfigListParameters(IntPtr handle)
    {
        

        List<(string name, Type type)> res = new();
        Mupen64PlusTypes.Error err = _fnConfigListParameters(handle, IntPtr.Zero, (_, name, type) => res.Add((name, type)));
        ThrowForError(err);

        return res;
    }

    public static void ConfigSaveFile()
    {
        
        Mupen64PlusTypes.Error err = _fnConfigSaveFile();
        ThrowForError(err);
    }

    public static void ConfigSaveSection(string name)
    {
        
        Mupen64PlusTypes.Error err = _fnConfigSaveSection(name);
        ThrowForError(err);
    }

    public static bool ConfigHasUnsavedChanges(string name)
    {
        
        return _fnConfigHasUnsavedChanges(name);
    }

    public static void ConfigDeleteSection(string name)
    {
        
        Mupen64PlusTypes.Error err = _fnConfigDeleteSection(name);
        ThrowForError(err);
    }

    public static void ConfigRevertChanges(string name)
    {
        
        Mupen64PlusTypes.Error err = _fnConfigRevertChanges(name);
        ThrowForError(err);
    }

    public static unsafe void ConfigSetInt(IntPtr handle, string name, int x)
    {
        

        Mupen64PlusTypes.Error err = _fnConfigSetParameter(handle, name, Mupen64PlusTypes.Type.Int, new IntPtr(&x));
        ThrowForError(err);
    }
    
    public static unsafe void ConfigSetFloat(IntPtr handle, string name, float x)
    {
        

        Mupen64PlusTypes.Error err = _fnConfigSetParameter(handle, name, Mupen64PlusTypes.Type.Float, new IntPtr(&x));
        ThrowForError(err);
    }
    
    public static unsafe void ConfigSetBool(IntPtr handle, string name, bool x)
    {
        

        Mupen64PlusTypes.Error err = _fnConfigSetParameter(handle, name, Mupen64PlusTypes.Type.Bool, new IntPtr(&x));
        ThrowForError(err);
    }

    public static void ConfigSetString(IntPtr handle, string name, string value)
    {
        

        IntPtr alloc = Marshal.StringToHGlobalAnsi(value);
        try
        {
            Mupen64PlusTypes.Error err = _fnConfigSetParameter(handle, name, Mupen64PlusTypes.Type.String, alloc);
            ThrowForError(err);
        }
        finally
        {
            Marshal.FreeHGlobal(alloc);
        }
    }

    public static void ConfigSetHelp(IntPtr handle, string name, string help)
    {
        

        Mupen64PlusTypes.Error err = _fnConfigSetParameterHelp(handle, name, help);
        ThrowForError(err);
    }

    public static Mupen64PlusTypes.Type ConfigGetType(IntPtr handle, string name)
    {
        Mupen64PlusTypes.Error err = _fnConfigGetParameterType(handle, name, out var type);
        if (err == Mupen64PlusTypes.Error.InputNotFound)
            throw new ArgumentOutOfRangeException(nameof(name), "Value not found in this config section");
        ThrowForError(err);

        return type;
    }
    
    [Pure]
    public static string? ConfigGetHelp(IntPtr handle, string name)
    {
        return _fnConfigGetParameterHelp(handle, name);
    }
    
    [Pure]
    public static int ConfigGetInt(IntPtr handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamInt(handle, name);
    }
    
    [Pure]
    public static float ConfigGetFloat(IntPtr handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamFloat(handle, name);
    }
    
    [Pure]
    public static bool ConfigGetBool(IntPtr handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamBool(handle, name);
    }
    
    [Pure]
    public static string ConfigGetString(IntPtr handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamString(handle, name);
    }

    #region Generic config get/set

    /// <summary>
    /// Retrieves a config value of generic type.
    /// </summary>
    /// <param name="handle">Handle to a config section.</param>
    /// <param name="name">The name of the parameter to retrieve.</param>
    /// <typeparam name="T">the type of data to retrieve.</typeparam>
    /// <returns>The value of the </returns>
    /// <exception cref="ArgumentException">If <typeparamref name="T"/> cannot be stored or retrieved from config.</exception>
    public static T ConfigGet<T>(IntPtr handle, string name)
    {
        var t = typeof(T);
        if (t == typeof(int) || (t.IsEnum && t.UnderlyingSystemType.IsIntegerType()))
            return (T) Convert.ChangeType(ConfigGetInt(handle, name), typeof(int));
        if (t == typeof(float) || t == typeof(double))
            return (T) Convert.ChangeType(ConfigGetFloat(handle, name), typeof(float));
        if (t == typeof(bool))
            return (T) Convert.ChangeType(ConfigGetBool(handle, name), typeof(bool));
        if (t == typeof(string))
            return (T) Convert.ChangeType(ConfigGetString(handle, name), typeof(string));
        throw new ArgumentException("Unknown type, cannot get generic config");
    }

    public static void ConfigSet<T>(IntPtr handle, string name, T value)
    {
        var t = typeof(T);
        if (t == typeof(int) || (t.IsEnum && t.UnderlyingSystemType.IsIntegerType()))
            ConfigSetInt(handle, name, (int) Convert.ChangeType(value, typeof(int))!);
        else if (t == typeof(float) || t == typeof(double))
            ConfigSetFloat(handle, name, (float) Convert.ChangeType(value, typeof(float))!);
        else if (t == typeof(bool))
            ConfigSetBool(handle, name, (bool) Convert.ChangeType(value, typeof(bool))!);
        else if (t == typeof(string))
            ConfigSetString(handle, name, (string) Convert.ChangeType(value, typeof(string))!);
        else
            throw new ArgumentException("Unknown type, cannot get generic config");
    }

    #endregion
}