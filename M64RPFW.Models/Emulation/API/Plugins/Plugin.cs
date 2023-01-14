using System.Diagnostics;
using System.Runtime.InteropServices;
using static M64RPFW.Models.Emulation.API.Mupen64PlusDelegates;
using static M64RPFW.Models.Emulation.API.Mupen64PlusTypes;

namespace M64RPFW.Models.Emulation.API.Plugins;

internal abstract class Plugin : IDisposable
{
    internal Plugin(EmulatorPluginType type, IntPtr handle)
    {
        Type = type;
        Handle = handle;

        if (TryGetDelegateFromLibrary(Handle, out PluginStartupDelegate? pluginStartup)) PluginStartup = pluginStartup;
        if (TryGetDelegateFromLibrary(Handle, out PluginShutdownDelegate? pluginShutdown))
            PluginShutdown = pluginShutdown;

        Debug.Print($"Loaded {Type}");
    }

    internal IntPtr Handle { get; }
    internal EmulatorPluginType Type { get; }
    internal bool IsAttached { get; private set; }

    internal PluginStartupDelegate? PluginStartup { get; }
    internal PluginShutdownDelegate? PluginShutdown { get; }

    void IDisposable.Dispose()
    {
        if (Handle != IntPtr.Zero)
        {
            NativeLibrary.Free(Handle);
            Debug.Print($"Freed {Type}");
        }
    }

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
}