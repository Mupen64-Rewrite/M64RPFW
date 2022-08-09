using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace M64RPFW.Models.Emulation.Core;

public static partial class Mupen64Plus
{
    public static IEnumerable<string> ConfigListSections()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        
        var res = new List<string>();
        Error err = _fnConfigListSections(IntPtr.Zero, (_, section) =>
        {
            res.Add(section);
        });
        ThrowForError(err);
        return res;
    }

    public static IntPtr ConfigOpenSection(string sectionName)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigOpenSection(sectionName, out IntPtr handle);
        ThrowForError(err);
        
        return handle;
    }

    public static IEnumerable<(string name, Type type)> ConfigListParameters(IntPtr section)
    {
        var res = new List<(string, Type)>();
        Error err = _fnConfigListParameters(section, IntPtr.Zero, (context, name, type) =>
        {
            res.Add((name, type));
        });
        return res;
    }

    public static bool ConfigHasUnsavedChanges(string sectionName)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        
        return _fnConfigHasUnsavedChanges(sectionName);
    }

    public static void ConfigDeleteSection(string sectionName)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        
        Error err = _fnConfigDeleteSection(sectionName);
        ThrowForError(err);
    }

    public static void ConfigSaveFile()
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigSaveFile();
        ThrowForError(err);
    }

    public static void ConfigSaveSection(string sectionName)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigSaveSection(sectionName);
        ThrowForError(err);
    }

    public static void ConfigRevertChanges(string sectionName)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);
        
        Error err = _fnConfigRevertChanges(sectionName);
        ThrowForError(err);
    }

    public static unsafe void ConfigSetParameter(IntPtr section, string paramName, int paramValue)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigSetParameter(section, paramName, Type.Int, &paramValue);
        ThrowForError(err);
    }

    public static unsafe void ConfigSetParameter(IntPtr section, string paramName, float paramValue)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigSetParameter(section, paramName, Type.Float, &paramValue);
        ThrowForError(err);
    }

    public static unsafe void ConfigSetParameter(IntPtr section, string paramName, bool paramValue)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        int actualValue = paramValue ? 1 : 0;
        
        Error err = _fnConfigSetParameter(section, paramName, Type.Bool, &actualValue);
        ThrowForError(err);
    }

    public static unsafe void ConfigSetParameter(IntPtr section, string paramName, string paramValue)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        IntPtr pValue = Marshal.StringToHGlobalAnsi(paramValue);
        try
        {
            Error err = _fnConfigSetParameter(section, paramName, Type.String, pValue.ToPointer());
        }
        finally
        {
            Marshal.FreeHGlobal(pValue);
        }
    }

    public static void ConfigSetParameterHelp(IntPtr section, string paramName, string help)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigSetParameterHelp(section, paramName, help);
        ThrowForError(err);
    }

    public static void ConfigSetDefault(IntPtr section, string paramName, int paramValue, string help) 
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigSetDefaultInt(section, paramName, paramValue, help);
        ThrowForError(err);
    }
    public static void ConfigSetDefault(IntPtr section, string paramName, float paramValue, string help) 
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigSetDefaultFloat(section, paramName, paramValue, help);
        ThrowForError(err);
    }
    public static void ConfigSetDefault(IntPtr section, string paramName, bool paramValue, string help) 
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigSetDefaultBool(section, paramName, paramValue, help);
        ThrowForError(err);
    }
    public static void ConfigSetDefault(IntPtr section, string paramName, string paramValue, string help) 
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigSetDefaultString(section, paramName, paramValue, help);
        ThrowForError(err);
    }

    public static Type ConfigGetType(IntPtr section, string paramName)
    {
        ThrowIfNotInited(MethodBase.GetCurrentMethod()!.Name);

        Error err = _fnConfigGetParameterType(section, paramName, out Type type);
        ThrowForError(err);
        return type;
    }

    public static int ConfigGetInt(IntPtr section, string paramName)
    {
        if (ConfigGetType(section, paramName) != Type.Int)
        {
            throw new InvalidOperationException($"Parameter {paramName} is not an int");
        }

        return _fnConfigGetParamInt(section, paramName);
    }
    public static float ConfigGetFloat(IntPtr section, string paramName)
    {
        if (ConfigGetType(section, paramName) != Type.Float)
        {
            throw new InvalidOperationException($"Parameter {paramName} is not an int");
        }

        return _fnConfigGetParamFloat(section, paramName);
    }
    public static bool ConfigGetBool(IntPtr section, string paramName)
    {
        if (ConfigGetType(section, paramName) != Type.Bool)
        {
            throw new InvalidOperationException($"Parameter {paramName} is not an int");
        }

        return _fnConfigGetParamBool(section, paramName);
    }
    public static string ConfigGetString(IntPtr section, string paramName)
    {
        if (ConfigGetType(section, paramName) != Type.String)
        {
            throw new InvalidOperationException($"Parameter {paramName} is not an int");
        }
        
        return _fnConfigGetParamString(section, paramName);
    }
    
    
}