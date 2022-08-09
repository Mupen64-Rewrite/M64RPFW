using System;
using System.Collections.ObjectModel;
using M64RPFW.Views;

namespace M64RPFW.Presenters;

internal class RecentRom
{
    public string Name { get; set; }
    public string Region { get; set; }
    public string Filename { get; set; }

}

internal partial class RecentRomPresenter
{
    public RecentRomPresenter(RecentRomView view, MainPresenter parent)
    {
        _view = view;
        _parent = parent;
        RecentRoms = new();
    }
    
    public void SelectAndRunROM(int index)
    {
        Console.WriteLine("SelectAndRunROM was called");
        _parent.SwitchTo(MainPresenter.SubView.Emulator);
        
    }

    private RecentRomView _view;
    private MainPresenter _parent;
    public ObservableCollection<RecentRom> RecentRoms { get; }
}