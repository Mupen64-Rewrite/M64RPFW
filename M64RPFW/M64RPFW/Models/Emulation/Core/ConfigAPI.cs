using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace M64RPFW.Models.Emulation.Core;

public static partial class Mupen64Plus
{
    public static IEnumerable<string> ListConfigSections()
    {
        ThrowIfNotInited();
        
        List<string> res = new();
        Error err = _fnConfigListSections(IntPtr.Zero, (_, name) => res.Add(name));
        ThrowForError(err);
        
        return res;
    }

    public static IntPtr ConfigOpenSection()
    {
        ThrowIfNotInited();
        
        Error err = _fnConfigOpenSection("Section", out IntPtr handle);
        ThrowForError(err);

        return handle;
    }

    public static IEnumerable<(string name, Type type)> ConfigListParameters(IntPtr handle)
    {
        ThrowIfNotInited();

        List<(string name, Type type)> res = new();
        Error err = _fnConfigListParameters(handle, IntPtr.Zero, (_, name, type) => res.Add((name, type)));
        ThrowForError(err);

        return res;
    }

    public static void ConfigSaveFile()
    {
        ThrowIfNotInited();
        Error err = _fnConfigSaveFile();
        ThrowForError(err);
    }

    public static void ConfigSaveSection(string name)
    {
        ThrowIfNotInited();
        Error err = _fnConfigSaveSection(name);
        ThrowForError(err);
    }

    public static bool ConfigHasUnsavedChanges(string name)
    {
        ThrowIfNotInited();
        return _fnConfigHasUnsavedChanges(name);
    }

    public static void ConfigDeleteSection(string name)
    {
        ThrowIfNotInited();
        Error err = _fnConfigDeleteSection(name);
        ThrowForError(err);
    }

    public static void ConfigRevertChanges(string name)
    {
        ThrowIfNotInited();
        Error err = _fnConfigRevertChanges(name);
        ThrowForError(err);
    }

    public static unsafe void ConfigSetInt(IntPtr handle, string name, int x)
    {
        ThrowIfNotInited();

        Error err = _fnConfigSetParameter(handle, name, Type.Int, new IntPtr(&x));
        ThrowForError(err);
    }
    
    public static unsafe void ConfigSetFloat(IntPtr handle, string name, float x)
    {
        ThrowIfNotInited();

        Error err = _fnConfigSetParameter(handle, name, Type.Float, new IntPtr(&x));
        ThrowForError(err);
    }
    
    public static unsafe void ConfigSetBool(IntPtr handle, string name, bool x)
    {
        ThrowIfNotInited();

        Error err = _fnConfigSetParameter(handle, name, Type.Bool, new IntPtr(&x));
        ThrowForError(err);
    }

    public static void ConfigSetString(IntPtr handle, string name, string value)
    {
        ThrowIfNotInited();

        IntPtr alloc = Marshal.StringToHGlobalAnsi(value);
        try
        {
            Error err = _fnConfigSetParameter(handle, name, Type.String, alloc);
            ThrowForError(err);
        }
        finally
        {
            Marshal.FreeHGlobal(alloc);
        }
    }

    public static void ConfigSetHelp(IntPtr handle, string name, string help)
    {
        ThrowIfNotInited();

        Error err = _fnConfigSetParameterHelp(handle, name, help);
        ThrowForError(err);
    }

    public static Type ConfigGetType(IntPtr handle, string name)
    {
        ThrowIfNotInited();

        Error err = _fnConfigGetParameterType(handle, name, out Type type);
        if (err == Error.InputNotFound)
            throw new ArgumentOutOfRangeException(nameof(name), "Value not found in this config section");
        ThrowForError(err);

        return type;
    }

    public static string? ConfigGetHelp(IntPtr handle, string name)
    {
        return _fnConfigGetParameterHelp(handle, name);
    }

    public static int ConfigGetInt(IntPtr handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamInt(handle, name);
    }
    
    public static float ConfigGetFloat(IntPtr handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamFloat(handle, name);
    }
    
    public static bool ConfigGetBool(IntPtr handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamBool(handle, name);
    }
    
    public static string ConfigGetString(IntPtr handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamString(handle, name);
    }
}