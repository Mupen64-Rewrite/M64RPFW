using System;
using System.IO;
using System.Threading;
using Eto.Drawing;
using M64RPFW.Models;
using M64RPFW.Models.Emulation.Core;

namespace M64RPFW.Presenters;

public class MainPresenter
{

    public MainPresenter()
    {
        _running = 0;
        _emuThread = null;
    }
    
    private void EmulatorThreadRun(object? param)
    {
        Interlocked.Exchange(ref _running, 1);

        RomFile rom = (RomFile) param!;
        
        try
        {
            rom.LoadThisRom();

            string pluginDir = Settings.PluginDir;
            string graphicsPlugin = Path.Join(pluginDir, Settings.GraphicsPlugin);
            string audioPlugin = Path.Join(pluginDir, Settings.AudioPlugin);
            string inputPlugin = Path.Join(pluginDir, Settings.InputPlugin);
            string rspPlugin = Path.Join(pluginDir, Settings.RspPlugin);
            
            // Mupen64Plus requires *this* particular order
            Mupen64Plus.AttachPlugin(graphicsPlugin);
            Mupen64Plus.AttachPlugin(audioPlugin);
            Mupen64Plus.AttachPlugin(inputPlugin);
            Mupen64Plus.AttachPlugin(rspPlugin);
            
            Mupen64Plus.Execute();
        }
        finally
        {
            Mupen64Plus.CloseRom();
            
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Graphics);
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Audio);
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.Input);
            Mupen64Plus.DetachPlugin(Mupen64Plus.PluginType.RSP);
            
            Interlocked.Exchange(ref _running, 0);
        }
    }

    public void LaunchRom(RomFile rom)
    {
        if (_running == 0)
        {
            _emuThread = new Thread(EmulatorThreadRun);
            _emuThread.Start(rom);
        }
    }
    
    private int _running;
    private Thread? _emuThread;
}