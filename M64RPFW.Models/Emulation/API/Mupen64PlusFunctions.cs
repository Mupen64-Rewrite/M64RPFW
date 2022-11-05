using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.API
{
    /// <summary>
    /// An <see cref="Attribute"/> containing metadata for Mupen64Plus functions
    /// </summary>
    [AttributeUsage(AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
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
            T? result = GetDelegateFromLibrary<T>(handle);
            @delegate = result;
            return @delegate != null;
        }

        #region Core

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreStartup")]
        internal delegate EmulatorStatus CoreStartupDelegate(int APIVersion, string ConfigPath, string DataPath, string Context, DebugCallbackDelegate DebugCallback, string context2, IntPtr dummy);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreShutdown")]
        internal delegate EmulatorStatus CoreShutdownDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreAttachPlugin")]
        internal delegate EmulatorStatus CoreAttachPluginDelegate(EmulatorPluginType PluginType, IntPtr PluginLibHandle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDetachPlugin")]
        internal delegate EmulatorStatus CoreDetachPluginDelegate(EmulatorPluginType PluginType);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("ConfigOpenSection")]
        internal delegate EmulatorStatus ConfigOpenSectionDelegate(string SectionName, ref IntPtr ConfigSectionHandle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("ConfigSetParameter")]
        internal delegate EmulatorStatus ConfigSetParameterIntDelegate(IntPtr ConfigSectionHandle, string ParamName, EmulatorTypes ParamType, ref int ParamValue);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("ConfigSetParameter")]
        internal delegate EmulatorStatus ConfigSetParameterStringDelegate(IntPtr ConfigSectionHandle, string ParamName, EmulatorTypes ParamType, StringBuilder ParamValue);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("ConfigSetParameter")]
        internal delegate EmulatorStatus ConfigSetParameterBoolDelegate(IntPtr ConfigSectionHandle, string ParamName, EmulatorTypes ParamType, ref bool ParamValue);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandByteArrayDelegate(EmulatorCommand Command, int ParamInt, byte[] ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandPointerDelegate(EmulatorCommand Command, int ParamInt, IntPtr ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandRefPointerDelegate(EmulatorCommand Command, ref int ParamInt, IntPtr ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandStringDelegate(EmulatorCommand Command, int ParamInt, string ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandRomHeaderDelegate(EmulatorCommand Command, int ParamInt, ref EmulatorRomHeader ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandRomSettingsDelegate(EmulatorCommand Command, EmulatorRomSettings ParamInt, ref int ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandCoreStateQueryDelegate(EmulatorCommand Command, EmulatorCoreParameters ParamInt, int ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandCoreStateSetDelegate(EmulatorCommand Command, EmulatorCoreParameters ParamInt, ref int ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandCoreStateSetVideoModeDelegate(EmulatorCommand Command, EmulatorVideoModes ParamInt, IntPtr ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandCoreStateSetRefDelegate(EmulatorCommand Command, EmulatorCoreParameters ParamInt, ref int ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandRefIntDelegate(EmulatorCommand Command, int ParamInt, ref int ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandFrameCallbackDelegate(EmulatorCommand Command, int ParamInt, FrameCallbackDelegate ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandVICallbackDelegate(EmulatorCommand Command, int ParamInt, VICallbackDelegate ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("CoreDoCommand")]
        internal delegate EmulatorStatus CoreDoCommandRenderCallbackDelegate(EmulatorCommand Command, int ParamInt, RenderCallbackDelegate ParamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("ConfigSetDefaultFloat")]
        internal delegate EmulatorStatus ConfigSetDefaultFloatDelegate(string ConfigSectionHandle, string ParamName, double ParamValue, string ParamHelp);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("ConfigSetDefaultString")]
        internal delegate EmulatorStatus ConfigSetDefaultStringDelegate(string ConfigSectionHandle, string ParamName, string ParamValue, string ParamHelp);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("ConfigSaveFile")]
        internal delegate EmulatorStatus ConfigSaveFileDelegate();

        #endregion

        #region Callbacks

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void FrameCallbackDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void VICallbackDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void RenderCallbackDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void StartupCallbackDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void DebugCallbackDelegate(IntPtr Context, int level, string Message);

        #endregion

        #region Plugins

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [LibraryFunction("PluginStartup")]
        internal delegate EmulatorStatus PluginStartupDelegate(IntPtr CoreHandle, string Context, DebugCallbackDelegate DebugCallback);

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
        internal delegate int GetScreenTextureIDDelegate();

        #endregion
    }
}
