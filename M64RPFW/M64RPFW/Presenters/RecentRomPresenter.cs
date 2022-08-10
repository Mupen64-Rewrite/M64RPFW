using System;
using System.Collections.ObjectModel;
using System.IO;
using M64RPFW.Models;
using M64RPFW.Models.Emulation.Core;
using M64RPFW.Views;

namespace M64RPFW.Presenters;

internal class RecentRomPresenter
{
    public RecentRomPresenter(RecentRomView view, MainView parent)
    {
        _view = view;
        _parent = parent.Presenter;

        RecentRoms = new();
    }
    
    public void SelectAndRunROM(int index)
    {
        _parent.LaunchRom(RecentRoms[index]);
    }

    private RecentRomView _view;
    private MainPresenter _parent;
    public ObservableCollection<RomFile> RecentRoms { get; }
}