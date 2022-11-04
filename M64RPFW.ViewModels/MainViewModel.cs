using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Interfaces;
using System.Collections.ObjectModel;

namespace M64RPFW.ViewModels
{
    public partial class MainViewModel : ObservableObject, IAppExitEventProvider
    {
        private event Action onWindowExit;
        void IAppExitEventProvider.Register(Action action) => onWindowExit += action;

        private readonly GeneralDependencyContainer generalDependencyContainer;

        public EmulatorViewModel EmulatorViewModel { get; }
        public RecentRomsViewModel RecentRomsViewModel { get; }

        public MainViewModel(GeneralDependencyContainer generalDependencyContainer)
        {
            this.generalDependencyContainer = generalDependencyContainer;

            RecentRomsViewModel = new(generalDependencyContainer);

            EmulatorViewModel = new(generalDependencyContainer, this, RecentRomsViewModel);
        }


        [RelayCommand]
        private void Exit()
        {
            onWindowExit?.Invoke();
        }

    }
}
