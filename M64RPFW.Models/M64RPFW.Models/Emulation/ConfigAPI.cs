using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace M64RPFW.Models.Emulation;

public static partial class Mupen64Plus
{
    public static IEnumerable<string> ListConfigSections()
    {
        List<string> res = new();
        var err = _fnConfigListSections(nint.Zero, (_, name) => res.Add(name));
        ThrowForError(err);

        return res;
    }

    public static nint ConfigOpenSection(string name)
    {
        var err = _fnConfigOpenSection(name, out var handle);
        ThrowForError(err);

        return handle;
    }

    public static IEnumerable<(string name, Type type)> ConfigListParameters(nint handle)
    {
        List<(string name, Type type)> res = new();
        var err = _fnConfigListParameters(handle, nint.Zero, (_, name, type) => res.Add((name, type)));
        ThrowForError(err);

        return res;
    }

    public static void ConfigSaveFile()
    {
        var err = _fnConfigSaveFile();
        ThrowForError(err);
    }

    public static void ConfigSaveSection(string name)
    {
        var err = _fnConfigSaveSection(name);
        ThrowForError(err);
    }

    public static bool ConfigHasUnsavedChanges(string name)
    {
        return _fnConfigHasUnsavedChanges(name);
    }

    public static void ConfigDeleteSection(string name)
    {
        var err = _fnConfigDeleteSection(name);
        ThrowForError(err);
    }

    public static void ConfigRevertChanges(string name)
    {
        var err = _fnConfigRevertChanges(name);
        ThrowForError(err);
    }

    public static unsafe void ConfigSetInt(nint handle, string name, int x)
    {
        var err = _fnConfigSetParameter(handle, name, Type.Int, new nint(&x));
        ThrowForError(err);
    }

    public static unsafe void ConfigSetFloat(nint handle, string name, float x)
    {
        var err = _fnConfigSetParameter(handle, name, Type.Float, new nint(&x));
        ThrowForError(err);
    }

    public static unsafe void ConfigSetBool(nint handle, string name, bool x)
    {
        var err = _fnConfigSetParameter(handle, name, Type.Bool, new nint(&x));
        ThrowForError(err);
    }

    public static void ConfigSetString(nint handle, string name, string value)
    {
        var alloc = Marshal.StringToHGlobalAnsi(value);
        try
        {
            var err = _fnConfigSetParameter(handle, name, Type.String, alloc);
            ThrowForError(err);
        }
        finally
        {
            Marshal.FreeHGlobal(alloc);
        }
    }

    public static void ConfigSetHelp(nint handle, string name, string help)
    {
        var err = _fnConfigSetParameterHelp(handle, name, help);
        ThrowForError(err);
    }

    public static Type ConfigGetType(nint handle, string name)
    {
        var err = _fnConfigGetParameterType(handle, name, out var type);
        if (err == Error.InputNotFound)
            throw new ArgumentOutOfRangeException(nameof(name), "Value not found in this config section");
        ThrowForError(err);

        return type;
    }

    [Pure]
    public static string? ConfigGetHelp(nint handle, string name)
    {
        return _fnConfigGetParameterHelp(handle, name);
    }

    [Pure]
    public static int ConfigGetInt(nint handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamInt(handle, name);
    }

    [Pure]
    public static float ConfigGetFloat(nint handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamFloat(handle, name);
    }

    [Pure]
    public static bool ConfigGetBool(nint handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamBool(handle, name);
    }

    [Pure]
    public static string ConfigGetString(nint handle, string name)
    {
        ConfigGetType(handle, name);
        return _fnConfigGetParamString(handle, name);
    }
}