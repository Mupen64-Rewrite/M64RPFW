using System;
using System.Reflection;
using System.Runtime.InteropServices;
using M64RPFW.Models.Emulation.Core;

namespace M64RPFW.Models;

public static class Settings
{
    private static IntPtr _configSection = IntPtr.Zero;

    private static void InitConfigSection()
    {
        if (_configSection != IntPtr.Zero)
            return;

        _configSection = Mupen64Plus.ConfigOpenSection("UI-RPFW");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Mupen64Plus.ConfigSetDefault(_configSection, "PluginDir", "/usr/lib/mupen64plus",
                "Directory where relative plugin paths are evaluated from");
            Mupen64Plus.ConfigSetDefault(_configSection, "GraphicsPlugin", "mupen64plus-video-rice.so",
                "Video plugin to use.");
            Mupen64Plus.ConfigSetDefault(_configSection, "AudioPlugin", "mupen64plus-audio-sdl.so",
                "Audio plugin to use.");
            Mupen64Plus.ConfigSetDefault(_configSection, "InputPlugin", "mupen64plus-input-sdl.so",
                "Input plugin to use.");
            Mupen64Plus.ConfigSetDefault(_configSection, "RspPlugin", "mupen64plus-rsp-hle.so",
                "RSP plugin to use.");
        }
    }

    public static string PluginDir
    {
        get
        {
            InitConfigSection();
            return Mupen64Plus.ConfigGetString(_configSection, nameof(PluginDir));
        }
        set => Mupen64Plus.ConfigSetString(_configSection, nameof(PluginDir), value);
    }

    public static string GraphicsPlugin
    {
        get
        {
            InitConfigSection();
            return Mupen64Plus.ConfigGetString(_configSection, nameof(GraphicsPlugin));
        }
        set => Mupen64Plus.ConfigSetString(_configSection, nameof(GraphicsPlugin), value);
    }
    
    public static string AudioPlugin
    {
        get
        {
            InitConfigSection();
            return Mupen64Plus.ConfigGetString(_configSection, nameof(AudioPlugin));
        }
        set => Mupen64Plus.ConfigSetString(_configSection, nameof(AudioPlugin), value);
    }
    
    public static string InputPlugin
    {
        get
        {
            InitConfigSection();
            return Mupen64Plus.ConfigGetString(_configSection, nameof(InputPlugin));
        }
        set => Mupen64Plus.ConfigSetString(_configSection, nameof(InputPlugin), value);
    }
    
    public static string RspPlugin
    {
        get
        {
            InitConfigSection();
            return Mupen64Plus.ConfigGetString(_configSection, nameof(RspPlugin));
        }
        set => Mupen64Plus.ConfigSetString(_configSection, nameof(RspPlugin), value);
    }
}