using System;
using System.ComponentModel;
using Eto;
using Eto.Forms;
using M64RPFW.Models.Emulation.Core;

namespace M64RPFW.Misc;

public class M64RPFWApplication : Application
{
    public M64RPFWApplication() 
    {
        MainForm = new MainView();
    }
    
    public M64RPFWApplication(string platform) : base(platform) 
    {
        MainForm = new MainView();
    }
    
    public M64RPFWApplication(Platform platform) : base(platform) 
    {
        MainForm = new MainView();
    }

    protected override void OnInitialized(EventArgs e)
    {
        Mupen64Plus.Startup();
        // Set a few defaults
        IntPtr config = Mupen64Plus.ConfigOpenSection("UI-RPFW");
        Mupen64Plus.ConfigSetDefault(config, "PluginDir", "/usr/lib/mupen64plus", "Plugin directory to use.");
        Mupen64Plus.ConfigSetDefault(config, "VideoPlugin", "mupen64plus-video-GLideN64.so", "Video plugin to use.");
        
        
        MainForm.Visible = true;
    }

    protected override void OnTerminating(CancelEventArgs e)
    {
        var emuState = (Mupen64Plus.EmuState) Mupen64Plus.CoreStateQuery(Mupen64Plus.CoreParam.EmuState);
        if (emuState != Mupen64Plus.EmuState.Stopped)
        {
            Mupen64Plus.Stop();
            Mupen64Plus.CloseROM();
            
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Graphics);
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Audio);
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Input);
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.RSP);
        }
        Mupen64Plus.Shutdown();
    }
    
    
}