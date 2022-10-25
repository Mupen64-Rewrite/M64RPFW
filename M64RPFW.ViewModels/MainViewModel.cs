using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using System.Collections.ObjectModel;

namespace M64RPFW.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        internal event Action OnWindowExit;

        private readonly GeneralDependencyContainer generalDependencyContainer;

        public EmulatorViewModel EmulatorViewModel { get; }
        public SavestatesViewModel SavestatesViewModel { get; }
        public RecentROMsViewModel RecentROMsViewModel { get; }

        public MainViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;

            RecentROMsViewModel = new(generalDependencyContainer);

            generalDependencyContainer.RecentRomsProvider = RecentROMsViewModel;

            EmulatorViewModel = new(generalDependencyContainer, this);
            SavestatesViewModel = new();
        }


        [RelayCommand]
        private void Exit()
        {
            OnWindowExit?.Invoke();
        }
    }
}
