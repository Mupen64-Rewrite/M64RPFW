using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using System.Collections.ObjectModel;

namespace M64RPFW.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly GeneralDependencyContainer generalDependencyContainer;

        public EmulatorViewModel EmulatorViewModel { get; private set; }
        public SavestatesViewModel SavestatesViewModel { get; private set; }
        public RecentROMsViewModel RecentROMsViewModel { get; private set; }

        [ObservableProperty]
        private ObservableCollection<ROMViewModel> recentROMs = new();

        public MainViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;

            RecentROMsViewModel = new(generalDependencyContainer);

            generalDependencyContainer.RecentRomsProvider = RecentROMsViewModel;

            EmulatorViewModel = new(generalDependencyContainer);
            SavestatesViewModel = new();

        }
    }
}
