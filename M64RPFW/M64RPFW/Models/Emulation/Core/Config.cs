using System;
using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using static M64RPFW.Models.Helpers.NativeLibHelper;


namespace M64RPFW.Models.Emulation.Core;

public static partial class Mupen64Plus
{
    #region Config API callbacks

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void SectionListCallback(IntPtr context, string section);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void ParameterListCallback(IntPtr context, string paramName, Type paramType);

    #endregion

    #region Config API delegates

    // Selector functions
    // ========================================================

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DConfigListSections(IntPtr context, SectionListCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DConfigOpenSection(string sectionName, out IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DConfigListParameters(IntPtr section, IntPtr context, ParameterListCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate bool DConfigHasUnsavedChanges(string sectionName);

    // Modifier functions
    // ========================================================

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DConfigDeleteSection(string sectionName);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DConfigSaveFile();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DConfigSaveSection(string sectionName);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DConfigRevertChanges(string sectionName);

    // Get/set functions
    // ========================================================

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private unsafe delegate Error DConfigSetParameter(IntPtr section, string paramName, Type paramType, void* value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DConfigSetParameterHelp(IntPtr section, string paramName, string paramHelp);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DConfigGetParameterType(IntPtr handle, string paramName, out Type paramType);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DConfigSetDefaultInt(IntPtr handle, string paramName, int paramValue, string paramHelp);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DConfigSetDefaultFloat(IntPtr handle, string paramName, float paramValue, string paramHelp);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error DConfigSetDefaultBool(IntPtr handle, string paramName, bool paramValue, string paramHelp);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Error
        DConfigSetDefaultString(IntPtr handle, string paramName, string paramValue, string paramHelp);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate int DConfigGetParamInt(IntPtr section, string paramName);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate float DConfigGetParamFloat(IntPtr section, string paramName);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate bool DConfigGetParamBool(IntPtr section, string paramName);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate string DConfigGetParamString(IntPtr section, string paramName);

    #endregion

    private static void ResolveConfigFunctions()
    {
        ResolveDelegate(_libHandle, out _fnConfigListSections);
        ResolveDelegate(_libHandle, out _fnConfigOpenSection);
        ResolveDelegate(_libHandle, out _fnConfigListParameters);
        ResolveDelegate(_libHandle, out _fnConfigHasUnsavedChanges);
        ResolveDelegate(_libHandle, out _fnConfigDeleteSection);
        ResolveDelegate(_libHandle, out _fnConfigSaveFile);
        ResolveDelegate(_libHandle, out _fnConfigSaveSection);
        ResolveDelegate(_libHandle, out _fnConfigRevertChanges);
        ResolveDelegate(_libHandle, out _fnConfigSetParameter);
        ResolveDelegate(_libHandle, out _fnConfigSetParameterHelp);
        ResolveDelegate(_libHandle, out _fnConfigGetParameterType);
        ResolveDelegate(_libHandle, out _fnConfigSetDefaultInt);
        ResolveDelegate(_libHandle, out _fnConfigSetDefaultFloat);
        ResolveDelegate(_libHandle, out _fnConfigSetDefaultBool);
        ResolveDelegate(_libHandle, out _fnConfigSetDefaultString);
        ResolveDelegate(_libHandle, out _fnConfigGetParamInt);
        ResolveDelegate(_libHandle, out _fnConfigGetParamFloat);
        ResolveDelegate(_libHandle, out _fnConfigGetParamBool);
        ResolveDelegate(_libHandle, out _fnConfigGetParamString);
    }

    private static DConfigListSections _fnConfigListSections;
    private static DConfigOpenSection _fnConfigOpenSection;
    private static DConfigListParameters _fnConfigListParameters;
    private static DConfigHasUnsavedChanges _fnConfigHasUnsavedChanges;
    private static DConfigDeleteSection _fnConfigDeleteSection;
    private static DConfigSaveFile _fnConfigSaveFile;
    private static DConfigSaveSection _fnConfigSaveSection;
    private static DConfigRevertChanges _fnConfigRevertChanges;
    private static DConfigSetParameter _fnConfigSetParameter;
    private static DConfigSetParameterHelp _fnConfigSetParameterHelp;
    private static DConfigGetParameterType _fnConfigGetParameterType;
    private static DConfigSetDefaultInt _fnConfigSetDefaultInt;
    private static DConfigSetDefaultFloat _fnConfigSetDefaultFloat;
    private static DConfigSetDefaultBool _fnConfigSetDefaultBool;
    private static DConfigSetDefaultString _fnConfigSetDefaultString;
    private static DConfigGetParamInt _fnConfigGetParamInt;
    private static DConfigGetParamFloat _fnConfigGetParamFloat;
    private static DConfigGetParamBool _fnConfigGetParamBool;
    private static DConfigGetParamString _fnConfigGetParamString;
}