using System;
using Eto.Drawing;
using M64RPFW.Models.Emulation.Core;

namespace M64RPFW.Presenters;

public class MainPresenter
{
    public MainPresenter(MainView view)
    {
        _view = view;
    }

    public enum SubView
    {
        RecentRom,
        Emulator
    }

    public void SwitchTo(SubView view)
    {
        switch (view)
        {
            case SubView.RecentRom:
                _view.MinimumSize = new Size(256, 144);
                _view.Content = _view.RomView;
                _view.Resizable = true;
                break;
            case SubView.Emulator:
                _view.Content = _view.EmuView;
                break;
        }
    }

    private readonly MainView _view;
}