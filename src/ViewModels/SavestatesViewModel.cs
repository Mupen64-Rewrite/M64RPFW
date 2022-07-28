using CommunityToolkit.Mvvm.ComponentModel;
using System;
using M64RPFWAvalonia.ViewModels.Interfaces;
using static M64RPFWAvalonia.Models.Emulation.Provider.GameInfoProvider;
using CommunityToolkit.Mvvm.Input;

namespace M64RPFWAvalonia.ViewModels
{
    public partial class SavestatesViewModel : ObservableObject
    {
        private int saveStateSlot = 0;
        public int SaveStateSlot { get => saveStateSlot; set => SetProperty(ref saveStateSlot, Math.Clamp(value, SAVESTATE_SLOT_MIN, SAVESTATE_SLOT_MAX)); }

        [RelayCommand]
        private void SetSaveStateSlot(string slot)
        {
            SaveStateSlot = int.Parse(slot);
        }

    }
}
