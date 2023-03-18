using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
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
}