using System.Threading;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Views;

namespace M64RPFW.Presenters;

public class EmulatorPresenter
{
    public EmulatorPresenter(EmulatorView view, MainPresenter parent)
    {
        _view = view;
        _parent = parent;
        _running = 0;
    }

    void EmulatorThreadRun(string romFile)
    {
        Interlocked.Exchange(ref _running, 1);
        try
        {
            Mupen64Plus.OpenROM(romFile);
            
            
            Mupen64Plus.CloseROM();
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
        
    }

    private int _running;
    private EmulatorView _view;
    private MainPresenter _parent;
}