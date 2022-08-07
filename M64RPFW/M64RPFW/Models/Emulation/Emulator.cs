using System.Threading;
using M64RPFW.Models.Emulation.Core;

namespace M64RPFW.Models.Emulation;

public class Emulator
{
    public Thread? Thread { get; private set; }
    public Mupen64Plus Core { get; private set; }

    public Emulator()
    {
        Core = new Mupen64Plus();
    }

    private struct EmuThreadArgs
    {
        public readonly string RomFile;
        public readonly (string video, string rsp, string audio, string input) Plugins;

        public EmuThreadArgs(string romFile, (string video, string rsp, string audio, string input) plugins)
        {
            RomFile = romFile;
            Plugins = plugins;
        }
    }

    public void Launch(string romFile, (string video, string rsp, string audio, string input) plugins)
    {
        Thread = new Thread(EmuThreadRun);
        Thread.Start(new EmuThreadArgs(romFile, plugins));
    }

    private void EmuThreadRun(object? args)
    {
        if (args == null)
            return;
        var targs = (EmuThreadArgs) args;
        Core.OpenROM(targs.RomFile);
        
        Core.AttachPlugin(targs.Plugins.video);
        Core.AttachPlugin(targs.Plugins.audio);
        Core.AttachPlugin(targs.Plugins.input);
        Core.AttachPlugin(targs.Plugins.rsp);
        
        Core.Execute();
        
        Core.CloseROM();
        
        Core.DetachPlugin(Mupen64Plus.PluginType.Graphics);
        Core.DetachPlugin(Mupen64Plus.PluginType.Audio);
        Core.DetachPlugin(Mupen64Plus.PluginType.Input);
        Core.DetachPlugin(Mupen64Plus.PluginType.RSP);
    }
    
    
}