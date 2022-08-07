using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.Views;

namespace M64RPFW.ViewModels;

internal class RecentROM
{
    public string Name { get; set; }
    public string Region { get; set; }
    public string Filename { get; set; }

}

internal partial class RecentRomViewModel
{
    public RecentRomViewModel()
    {
        RomObjects = new();
    }
    
    [RelayCommand]
    public void SelectAndRunROM(int index)
    {
        Console.WriteLine("SelectAndRunROM was called");
    }
    
    public ObservableCollection<RecentROM> RomObjects { get; }
}