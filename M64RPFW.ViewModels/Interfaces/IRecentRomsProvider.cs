using System.Collections.ObjectModel;

namespace M64RPFW.ViewModels.Interfaces
{
    public interface IRecentRomsProvider
    {
        public ObservableCollection<ROMViewModel> GetRecentRoms();
        public void AddRecentROM(ROMViewModel rom);
    }
}
