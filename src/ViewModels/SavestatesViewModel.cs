using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static M64RPFW.Models.Emulation.Provider.GameInfoProvider;

namespace M64RPFW.ViewModels
{
    public partial class SavestatesViewModel : ObservableObject
    {
        private int saveStateSlot = 0;
        public int SaveStateSlot { get => saveStateSlot; set => SetProperty(ref saveStateSlot, Math.Clamp(value, SAVESTATE_SLOT_MIN, SAVESTATE_SLOT_MAX)); }

    }
}
