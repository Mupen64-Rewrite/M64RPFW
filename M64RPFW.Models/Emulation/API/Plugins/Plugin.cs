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
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.API.Plugins
{
    internal abstract class Plugin : IDisposable
    {
        internal Plugin(EmulatorPluginType type, IntPtr handle)
        {
            Type = type;
            Handle = handle;

            if (TryGetDelegateFromLibrary<PluginStartupDelegate>(Handle, out PluginStartupDelegate? pluginStartup))
            {
                PluginStartup = pluginStartup;
            }
            if (TryGetDelegateFromLibrary<PluginShutdownDelegate>(Handle, out PluginShutdownDelegate? pluginShutdown))
            {
                PluginShutdown = pluginShutdown;
            }

            Debug.Print($"Loaded {Type}");
        }

        internal IntPtr Handle { get; private set; }
        internal EmulatorPluginType Type { get; }
        internal bool IsAttached { get; private set; }

        internal PluginStartupDelegate? PluginStartup { get; private set; }
        internal PluginShutdownDelegate? PluginShutdown { get; private set; }

        internal virtual void Attach(CorePlugin? corePlugin)
        {
            IsAttached = true;

            if (corePlugin != null)
            {
                PluginStartup?.Invoke(corePlugin.Handle, null, null);
                corePlugin?.AttachPlugin(Type, Handle);
            }
        }

        internal virtual void Detach(CorePlugin? corePlugin)
        {
            IsAttached = false;

            if (corePlugin != null)
            {
                PluginShutdown?.Invoke();
                corePlugin?.DetachPlugin(Type);
            }
        }

        void IDisposable.Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                NativeLibrary.Free(Handle);
                Debug.Print($"Freed {Type}");
            }
        }
    }
}
