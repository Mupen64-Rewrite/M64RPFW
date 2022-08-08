using M64RPFW.Models.Emulation.Core;
using M64RPFW.Views;

namespace M64RPFW.Presenters;

public class EmulatorPresenter
{
    public EmulatorPresenter(EmulatorView view)
    {
        _view = view;

        core = new Mupen64Plus();
    }

    void EmulatorThreadRun(string s)
    {
        core.OpenROM(s);
        
        
        core.CloseROM();
        
    }

    private Mupen64Plus core;
    private EmulatorView _view;
}