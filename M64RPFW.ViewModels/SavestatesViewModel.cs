using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.ViewModels.Containers;
using System;

namespace M64RPFW.ViewModels
{
    public partial class SavestatesViewModel : ObservableObject
    {
        private readonly GeneralDependencyContainer? generalDependencyContainer;

        private int saveStateSlot = 0;
        public int SaveStateSlot { get => saveStateSlot; set => SetProperty(ref saveStateSlot, Math.Clamp(value, 0, generalDependencyContainer.SavestateBoundsConfigurationProvider.SavestateBoundsConfiguration.Slots)); }

    }
}
