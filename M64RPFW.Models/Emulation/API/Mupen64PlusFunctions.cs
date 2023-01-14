using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.API;

/// <summary>
///     An <see cref="Attribute" /> containing metadata for Mupen64Plus functions
/// </summary>
[AttributeUsage(AttributeTargets.Delegate)]
internal sealed class LibraryFunctionAttribute : Attribute
{
    public LibraryFunctionAttribute(string exportSymbol)
    {
        ExportSymbol = exportSymbol;
    }

    public string ExportSymbol { get; }
}

internal static class Mupen64PlusDelegates
{
    [DllImport("kernel32.dll")]
    internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    internal static T? GetDelegateFromLibrary<T>(IntPtr handle) where T : Delegate
    {
        try
        {
            return (T)Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(handle,
                    typeof(T).GetCustomAttribute<LibraryFunctionAttribute>()!.ExportSymbol),
                typeof(T));
        }
        catch
        {
            return null;
        }
    }

    internal static bool TryGetDelegateFromLibrary<T>(IntPtr handle, out T @delegate) where T : Delegate
    {
        var result = GetDelegateFromLibrary<T>(handle);
        @delegate = result;
        return @delegate != null;
    }

    #region Core

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreStartup")]
    internal delegate EmulatorStatus CoreStartupDelegate(int apiVersion, string configPath, string dataPath,
        string context, DebugCallbackDelegate debugCallback, string context2, IntPtr dummy);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreShutdown")]
    internal delegate EmulatorStatus CoreShutdownDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreAttachPlugin")]
    internal delegate EmulatorStatus CoreAttachPluginDelegate(EmulatorPluginType pluginType, IntPtr pluginLibHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDetachPlugin")]
    internal delegate EmulatorStatus CoreDetachPluginDelegate(EmulatorPluginType pluginType);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("ConfigOpenSection")]
    internal delegate EmulatorStatus ConfigOpenSectionDelegate(string sectionName, ref IntPtr configSectionHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("ConfigSetParameter")]
    internal delegate EmulatorStatus ConfigSetParameterIntDelegate(IntPtr configSectionHandle, string paramName,
        EmulatorTypes paramType, ref int paramValue);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("ConfigSetParameter")]
    internal delegate EmulatorStatus ConfigSetParameterStringDelegate(IntPtr configSectionHandle, string paramName,
        EmulatorTypes paramType, StringBuilder paramValue);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("ConfigSetParameter")]
    internal delegate EmulatorStatus ConfigSetParameterBoolDelegate(IntPtr configSectionHandle, string paramName,
        EmulatorTypes paramType, ref bool paramValue);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandByteArrayDelegate(EmulatorCommand command, int paramInt,
        byte[] paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandPointerDelegate(EmulatorCommand command, int paramInt,
        IntPtr paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandRefPointerDelegate(EmulatorCommand command, ref int paramInt,
        IntPtr paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus
        CoreDoCommandStringDelegate(EmulatorCommand command, int paramInt, string paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandRomHeaderDelegate(EmulatorCommand command, int paramInt,
        ref EmulatorRomHeader paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandRomSettingsDelegate(EmulatorCommand command,
        EmulatorRomSettings paramInt, ref int paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandCoreStateQueryDelegate(EmulatorCommand command,
        EmulatorCoreParameters paramInt, int paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandCoreStateSetDelegate(EmulatorCommand command,
        EmulatorCoreParameters paramInt, ref int paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandCoreStateSetVideoModeDelegate(EmulatorCommand command,
        EmulatorVideoModes paramInt, IntPtr paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandCoreStateSetRefDelegate(EmulatorCommand command,
        EmulatorCoreParameters paramInt, ref int paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandRefIntDelegate(EmulatorCommand command, int paramInt,
        ref int paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandFrameCallbackDelegate(EmulatorCommand command, int paramInt,
        FrameCallbackDelegate paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandViCallbackDelegate(EmulatorCommand command, int paramInt,
        ViCallbackDelegate paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("CoreDoCommand")]
    internal delegate EmulatorStatus CoreDoCommandRenderCallbackDelegate(EmulatorCommand command, int paramInt,
        RenderCallbackDelegate paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("ConfigSetDefaultFloat")]
    internal delegate EmulatorStatus ConfigSetDefaultFloatDelegate(string configSectionHandle, string paramName,
        double paramValue, string paramHelp);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("ConfigSetDefaultString")]
    internal delegate EmulatorStatus ConfigSetDefaultStringDelegate(string configSectionHandle, string paramName,
        string paramValue, string paramHelp);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("ConfigSaveFile")]
    internal delegate EmulatorStatus ConfigSaveFileDelegate();

    #endregion

    #region Callbacks

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void FrameCallbackDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ViCallbackDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void RenderCallbackDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void StartupCallbackDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DebugCallbackDelegate(IntPtr context, int level, string message);

    #endregion

    #region Plugins

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("PluginStartup")]
    internal delegate EmulatorStatus PluginStartupDelegate(IntPtr coreHandle, string context,
        DebugCallbackDelegate debugCallback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("PluginShutdown")]
    internal delegate EmulatorStatus PluginShutdownDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("ReadScreen2")]
    internal delegate void ReadScreen2Delegate(int[] framebuffer, ref int width, ref int height, int buffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("ReadScreen2")]
    internal delegate void ReadScreen2ResDelegate(IntPtr dummy, ref int width, ref int height, int buffer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [LibraryFunction("GetScreenTextureID")]
    internal delegate int GetScreenTextureIdDelegate();

    #endregion
}