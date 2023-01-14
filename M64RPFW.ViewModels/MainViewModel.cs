using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Interfaces;

namespace M64RPFW.ViewModels;

public partial class MainViewModel : ObservableObject, IAppExitEventProvider
{
    private readonly GeneralDependencyContainer generalDependencyContainer;

    public MainViewModel(GeneralDependencyContainer generalDependencyContainer)
    {
        this.generalDependencyContainer = generalDependencyContainer;

        RecentRomsViewModel = new RecentRomsViewModel(generalDependencyContainer);

        EmulatorViewModel = new EmulatorViewModel(generalDependencyContainer, this, RecentRomsViewModel);
    }

    public EmulatorViewModel EmulatorViewModel { get; }
    public RecentRomsViewModel RecentRomsViewModel { get; }

    void IAppExitEventProvider.Register(Action action)
    {
        onWindowExit += action;
    }

    private event Action onWindowExit;


    [RelayCommand]
    private void Exit()
    {
        onWindowExit?.Invoke();
    }
}