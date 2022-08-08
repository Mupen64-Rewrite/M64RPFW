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
    public RecentRomPresenter(RecentRomView view)
    {
        _view = view;
        RecentRoms = new();
    }
    
    public void SelectAndRunROM(int index)
    {
        Console.WriteLine("SelectAndRunROM was called");
    }

    private RecentRomView _view;
    public ObservableCollection<RecentRom> RecentRoms { get; }
}