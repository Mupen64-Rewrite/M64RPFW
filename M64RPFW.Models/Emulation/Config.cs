using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using M64RPFW.Models.Types;

namespace M64RPFW.Models.Emulation;

public static partial class Mupen64Plus
{
    #region Callbacks for config functions

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate void ListSectionsCallback(IntPtr context, string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate void ListParametersCallback(IntPtr context, string name, Type type);

    #endregion
    #region Delegates for config functions

    #region Section manipulation

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigListSections(IntPtr context, ListSectionsCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigOpenSection(string name, out IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigListParameters(IntPtr handle, IntPtr context, ListParametersCallback callback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigSaveFile();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigSaveSection(string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate bool DConfigHasUnsavedChanges(string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigDeleteSection(string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigRevertChanges(string name);

    #endregion

    #region Parameter setters/getters

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigSetParameter(IntPtr handle, string param, Mupen64PlusTypes.Type type, IntPtr value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigSetParameterHelp(IntPtr handle, string param, string help);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate Mupen64PlusTypes.Error DConfigGetParameterType(IntPtr handle, string param, out Mupen64PlusTypes.Type type);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate string? DConfigGetParameterHelp(IntPtr handle, string param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate int DConfigGetParamInt(IntPtr handle, string param);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate float DConfigGetParamFloat(IntPtr handle, string param);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate bool DConfigGetParamBool(IntPtr handle, string param);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [RuntimeDllImport]
    private delegate string DConfigGetParamString(IntPtr handle, string param);

    #endregion

    #endregion
    
    #pragma warning disable CS8618
    
    private static DConfigListSections _fnConfigListSections;
    private static DConfigOpenSection _fnConfigOpenSection;
    private static DConfigListParameters _fnConfigListParameters;
    private static DConfigSaveFile _fnConfigSaveFile;
    private static DConfigSaveSection _fnConfigSaveSection;
    private static DConfigHasUnsavedChanges _fnConfigHasUnsavedChanges;
    private static DConfigDeleteSection _fnConfigDeleteSection;
    private static DConfigRevertChanges _fnConfigRevertChanges;
    private static DConfigSetParameter _fnConfigSetParameter;
    private static DConfigSetParameterHelp _fnConfigSetParameterHelp;
    private static DConfigGetParameterType _fnConfigGetParameterType;
    private static DConfigGetParameterHelp _fnConfigGetParameterHelp;
    private static DConfigGetParamInt _fnConfigGetParamInt;
    private static DConfigGetParamFloat _fnConfigGetParamFloat;
    private static DConfigGetParamBool _fnConfigGetParamBool;
    private static DConfigGetParamString _fnConfigGetParamString;
    
    #pragma warning restore CS8168
    
    private static void ResolveConfigFunctions()
    {
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigListSections);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigOpenSection);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigListParameters);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigSaveFile);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigSaveSection);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigHasUnsavedChanges);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigDeleteSection);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigRevertChanges);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigSetParameter);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigSetParameterHelp);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigGetParameterType);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigGetParameterHelp);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigGetParamInt);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigGetParamFloat);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigGetParamBool);
        NativeLibHelper.ResolveDelegate(_libHandle, out _fnConfigGetParamString);
    }
}