using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mupen64PlusRR.Models.Helpers;
using static Mupen64PlusRR.Models.Helpers.NativeLibHelper;
using IntPtr = System.IntPtr;

namespace Mupen64PlusRR.Models.Emulation;

public static partial class Mupen64Plus
{
    #region Delegates for frontend API

    // Callback types for the frontend API
    // ========================================================

    /// <summary>
    /// Logs a message from Mupen64Plus or an attached plugin.
    /// </summary>
    /// <param name="context">the pointer provided as debugContext to CoreStartup</param>
    /// <param name="level">the message's logging level</param>
    /// <param name="message">the message to log</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugCallback(IntPtr context, MessageLevel level, string message);

    /// <summary>
    /// Responds to a Mupen64Plus core parameter changing.
    /// </summary>
    /// <param name="context">the pointer provided as stateContext to CoreStartup</param>
    /// <param name="param">the core parameter that changed</param>
    /// <param name="newValue">the core parameter's new value</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void StateCallback(IntPtr context, CoreParam param, int newValue);
    
    /// <summary>
    /// Responds to the emulator completing a frame.
    /// </summary>
    /// <param name="index">the current frame index</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FrameCallback(int index);


    // Delegate types for core functions
    // ========================================================

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return : MarshalAs(UnmanagedType.LPStr)]
    [RuntimeDllImport]
    private delegate string DCoreErrorMessage(Error code);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreStartup(int apiVersion, string? configPath, string? dataPath,
        IntPtr debugContext, DebugCallback? debugCallback, IntPtr stateContext, StateCallback? stateCallback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreShutdown();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreAttachPlugin(PluginType pluginType, IntPtr pluginLibHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreDetachPlugin(PluginType pluginType);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private unsafe delegate Error DCoreDoCommand(Command cmd, int paramInt, void* paramPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreOverrideVidExt(VideoExtensionFunctions videoFunctionStruct);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private unsafe delegate Error DPluginGetVersion(out PluginType type, out int version, out int apiVersion, out byte* name,
        out int caps);

    #endregion

    // Plugin delegates
    // ========================================================
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DPluginStartup(IntPtr library, IntPtr debugContext, DebugCallback? debugCallback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DPluginShutdown();

#pragma warning disable CS8618
    private static Dictionary<PluginType, IntPtr> _pluginDict;
    
    // Frontend function utilities
    // ========================================================

    private static void ResolveFrontendFunctions()
    {
        ResolveDelegate(_libHandle, out _fnCoreErrorMessage);
        ResolveDelegate(_libHandle, out _fnCorePluginGetVersion);
        ResolveDelegate(_libHandle, out _fnCoreStartup);
        ResolveDelegate(_libHandle, out _fnCoreShutdown);
        ResolveDelegate(_libHandle, out _fnCoreAttachPlugin);
        ResolveDelegate(_libHandle, out _fnCoreDetachPlugin);
        ResolveDelegate(_libHandle, out _fnCoreOverrideVidExt);
        ResolveDelegate(_libHandle, out _fnCoreDoCommand);
    }

    // Frontend function members
    // ========================================================
    
    private static DCoreErrorMessage _fnCoreErrorMessage;
    private static DPluginGetVersion _fnCorePluginGetVersion;
    private static DCoreStartup _fnCoreStartup;
    private static DCoreShutdown _fnCoreShutdown;
    private static DCoreAttachPlugin _fnCoreAttachPlugin;
    private static DCoreDetachPlugin _fnCoreDetachPlugin;
    private static DCoreDoCommand _fnCoreDoCommand;
    private static DCoreOverrideVidExt _fnCoreOverrideVidExt;
#pragma warning restore CS8618
}