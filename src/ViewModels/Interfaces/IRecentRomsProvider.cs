using M64RPFWAvalonia.UI.ViewModels;
using System.Collections.ObjectModel;

namespace M64RPFWAvalonia.ViewModels.Interfaces
{
    public interface IRecentRomsProvider
    {
        public ObservableCollection<ROMViewModel> GetRecentRoms();
        public void AddRecentROM(ROMViewModel rom);
    }
}
