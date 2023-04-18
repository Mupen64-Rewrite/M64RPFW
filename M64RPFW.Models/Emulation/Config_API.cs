using System.Diagnostics.Contracts;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.Models.Emulation;

public static partial class Mupen64Plus
{
    public static void ConfigForEachSection(Action<string> callback)
    {
        var err = _fnConfigListSections(IntPtr.Zero, (_, name) => callback(name));
        ThrowForError(err);
    }

    public static IntPtr ConfigOpenSection(string name)
    {
        Mupen64PlusTypes.Error err = _fnConfigOpenSection(name, out IntPtr handle);
        ThrowForError(err);

        return handle;
    }

    public static void ConfigForEachParameter(IntPtr handle, Action<string, Mupen64PlusTypes.Type> func)
    {
        var err = _fnConfigListParameters(handle, IntPtr.Zero, (_, name, type) => func(name, type));
        ThrowForError(err);
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

    public static Mupen64PlusTypes.Type? ConfigGetType(IntPtr handle, string name)
    {
        Mupen64PlusTypes.Error err = _fnConfigGetParameterType(handle, name, out var type);
        if (err == Mupen64PlusTypes.Error.InputNotFound)
            return null;
        ThrowForError(err);

        return type;
    }
    
    [Pure]
    public static string? ConfigGetHelp(IntPtr handle, string name)
    {
        return Marshal.PtrToStringAnsi(_fnConfigGetParameterHelp(handle, name));
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
        IntPtr str = _fnConfigGetParamString(handle, name);
        return Marshal.PtrToStringAnsi(str)!;
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
    
    /// <summary>
    /// Retrieves a config value using a type specified by config.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static object? ConfigGetObject(IntPtr handle, string name)
    {
        ThrowForError(_fnConfigGetParameterType(handle, name, out var type));
        return type switch
        {
            Mupen64PlusTypes.Type.Int => _fnConfigGetParamInt(handle, name),
            Mupen64PlusTypes.Type.Float => _fnConfigGetParamFloat(handle, name),
            Mupen64PlusTypes.Type.Bool => _fnConfigGetParamBool(handle, name),
            Mupen64PlusTypes.Type.String => Marshal.PtrToStringAnsi(_fnConfigGetParamString(handle, name)),
            _ => null
        };
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
            throw new ArgumentException("Unknown type, cannot set generic config");
    }

    public static void ConfigSetObject(IntPtr handle, string name, object value)
    {
        var t = value.GetType();
        if (t == typeof(int) || (t.IsEnum && t.UnderlyingSystemType.IsIntegerType()))
            ConfigSetInt(handle, name, (int) Convert.ChangeType(value, typeof(int))!);
        else if (t == typeof(float) || t == typeof(double))
            ConfigSetFloat(handle, name, (float) Convert.ChangeType(value, typeof(float))!);
        else if (t == typeof(bool))
            ConfigSetBool(handle, name, (bool) Convert.ChangeType(value, typeof(bool))!);
        else if (t == typeof(string))
            ConfigSetString(handle, name, (string) Convert.ChangeType(value, typeof(string))!);
        else
            throw new ArgumentException("Unknown type, cannot set generic config");
    }

    #endregion
}