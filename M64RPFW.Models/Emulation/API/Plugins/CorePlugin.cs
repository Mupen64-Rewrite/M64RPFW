using M64RPFW.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static M64RPFW.Models.Emulation.API.Mupen64PlusDelegates;

namespace M64RPFW.Models.Emulation.API.Plugins
{
    internal class CorePlugin : Plugin
    {
        public CorePlugin(Mupen64PlusTypes.EmulatorPluginType type, IntPtr handle) : base(type, handle)
        {
            foreach (PropertyInfo item in GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                LibraryFunctionAttribute? attrib = item.PropertyType.GetCustomAttribute<LibraryFunctionAttribute>();
                if (attrib != null)
                {
                    object deleg = Convert.ChangeType(Marshal.GetDelegateForFunctionPointer(NativeLibrary.GetExport(handle,
                    item.PropertyType.GetCustomAttribute<LibraryFunctionAttribute>()!.ExportSymbol),
                    item.PropertyType), item.PropertyType);

                    item.SetValue(this, deleg);
                }
                else
                {
                    throw new Exception(); // TODO: add proper exception
                }
            }
        }

        internal CoreStartupDelegate Startup { get; private set; }
        internal CoreShutdownDelegate Shutdown { get; private set; }
        internal CoreAttachPluginDelegate AttachPlugin { get; private set; }
        internal CoreDetachPluginDelegate DetachPlugin { get; private set; }
        internal ConfigOpenSectionDelegate ConfigOpenSection { get; private set; }
        internal ConfigSetParameterIntDelegate ConfigSetParameterInt { get; private set; }
        internal ConfigSetParameterStringDelegate ConfigSetParameterString { get; private set; }
        internal ConfigSetParameterBoolDelegate ConfigSetParameterBool { get; private set; }
        internal CoreDoCommandByteArrayDelegate DoCommandByteArray { get; private set; }
        internal CoreDoCommandPointerDelegate DoCommandPointer { get; private set; }
        internal CoreDoCommandRefPointerDelegate DoCommandRefPointer { get; private set; }
        internal CoreDoCommandStringDelegate DoCommandString { get; private set; }
        internal CoreDoCommandRomHeaderDelegate DoCommandRomHeader { get; private set; }
        internal CoreDoCommandRomSettingsDelegate DoCommandRomSettings { get; private set; }
        internal CoreDoCommandCoreStateQueryDelegate DoCommandCoreStateQuery { get; private set; }
        internal CoreDoCommandCoreStateSetDelegate DoCommandCoreStateSet { get; private set; }
        internal CoreDoCommandCoreStateSetVideoModeDelegate DoCommandCoreStateSetVideoMode { get; private set; }
        internal CoreDoCommandCoreStateSetRefDelegate DoCommandCoreStateSetRef { get; private set; }
        internal CoreDoCommandRefIntDelegate DoCommandRefInt { get; private set; }
        internal ConfigSetDefaultFloatDelegate ConfigSetDefaultFloat { get; private set; }
        internal ConfigSetDefaultStringDelegate ConfigSetDefaultString { get; private set; }
        internal ConfigSaveFileDelegate ConfigSaveFile { get; private set; }

        internal CoreDoCommandFrameCallbackDelegate DoCommandFrameCallback { get; private set; }
        internal CoreDoCommandVICallbackDelegate DoCommandVICallback { get; private set; }
        internal CoreDoCommandRenderCallbackDelegate DoCommandRenderCallback { get; private set; }

    }
}
