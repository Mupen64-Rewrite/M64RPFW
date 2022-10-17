using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.src.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.ViewModels
{
    public partial class SavestatesViewModel : ObservableObject
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;

        private int saveStateSlot = 0;
        public int SaveStateSlot { get => saveStateSlot; set => SetProperty(ref saveStateSlot, Math.Clamp(value, 0, generalDependencyContainer.SavestateBoundsConfigurationProvider.SavestateBoundsConfiguration.Slots)); }

    }
}
