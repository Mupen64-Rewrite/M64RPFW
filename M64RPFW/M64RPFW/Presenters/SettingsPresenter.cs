using System;
using System.Runtime.InteropServices;
using Eto.Forms;
using M64RPFW.Models.Settings;
using M64RPFW.Views;

namespace M64RPFW.Presenters;

internal class SettingsPresenter
{
    static SettingsPresenter()
    {
        string? ext = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            ext = ".dll";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            ext = ".dylib";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            ext = ".so";

        if (ext == null)
            throw new NotSupportedException(".NET can apparently run on FreeBSD. We don't support it.");
            
        PluginFilter = new FileFilter("Plugins", ext);
    }
    
    public static FileFilter PluginFilter { get; }
    
    public SettingsPresenter(SettingsView view)
    {
        _view = view;
        _view.Closed += (_, _) =>
        {
            Settings.EnsureSaved();
        };
    }

    private readonly SettingsView _view;
}