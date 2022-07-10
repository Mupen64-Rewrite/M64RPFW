using M64RPFW.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.ViewModels.Interfaces
{
    public interface IRecentRomsProvider
    {
        public ObservableCollection<ROMViewModel> GetRecentRoms();
        public void AddRecentROM(ROMViewModel rom);
    }
}
