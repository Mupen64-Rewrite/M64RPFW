using System.Runtime.InteropServices;
using M64RPFW.Models.Helpers;
using static M64RPFW.Models.Helpers.NativeLibHelper;

namespace M64RPFW.Models.Emulation;

public static partial class Mupen64Plus
{
    // Plugin delegates
    // ========================================================
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DPluginStartup(nint library, nint debugContext, DebugCallback? debugCallback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate Error DPluginShutdown();

    #region Delegates for frontend API

    // Callback types for the frontend API
    // ========================================================

    /// <summary>
    ///     Logs a message from Mupen64Plus or an attached plugin.
    /// </summary>
    /// <param name="context">the pointer provided as debugContext to CoreStartup</param>
    /// <param name="level">the message's logging level</param>
    /// <param name="message">the message to log</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugCallback(nint context, MessageLevel level, string message);

    /// <summary>
    ///     Responds to a Mupen64Plus core parameter changing.
    /// </summary>
    /// <param name="context">the pointer provided as stateContext to CoreStartup</param>
    /// <param name="param">the core parameter that changed</param>
    /// <param name="newValue">the core parameter's new value</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void StateCallback(nint context, CoreParam param, int newValue);

    /// <summary>
    ///     Responds to the emulator completing a frame.
    /// </summary>
    /// <param name="index">the current frame index</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FrameCallback(int index);


    // Delegate types for core functions
    // ========================================================

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    [RuntimeDllImport]
    private delegate string DCoreErrorMessage(Error code);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreStartup(int apiVersion, string? configPath, string? dataPath,
        nint debugContext, DebugCallback? debugCallback, nint stateContext, StateCallback? stateCallback);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreShutdown();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [RuntimeDllImport]
    private delegate Error DCoreAttachPlugin(PluginType pluginType, nint pluginLibHandle);

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
    private unsafe delegate Error DPluginGetVersion(out PluginType type, out int version, out int apiVersion,
        out byte* name,
        out int caps);

    #endregion

#pragma warning disable CS8618
    private static readonly Dictionary<PluginType, nint> _pluginDict;

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