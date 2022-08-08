using System;
using Eto.Drawing;

namespace M64RPFW.Presenters;

public class MainPresenter
{
    public MainPresenter(MainView view)
    {
        _view = view;
    }

    enum SubView
    {
        RecentRom,
        Emulator
    }

    private void SwitchTo(SubView view)
    {
        switch (view)
        {
            case SubView.RecentRom:
                _view.MinimumSize = new Size(256, 144);
                _view.Content = _view.RomView;
                break;
            case SubView.Emulator:
                break;
        }
    }

    private readonly MainView _view;
}