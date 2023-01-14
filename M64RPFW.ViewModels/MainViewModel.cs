using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Interfaces;

namespace M64RPFW.ViewModels;

public partial class MainViewModel : ObservableObject, IAppExitEventProvider
{
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    public MainViewModel(GeneralDependencyContainer generalDependencyContainer)
    {
        this._generalDependencyContainer = generalDependencyContainer;

        RecentRomsViewModel = new RecentRomsViewModel(generalDependencyContainer);

        EmulatorViewModel = new EmulatorViewModel(generalDependencyContainer, this);
    }

    public EmulatorViewModel EmulatorViewModel { get; }
    public RecentRomsViewModel RecentRomsViewModel { get; }

    void IAppExitEventProvider.Register(Action action)
    {
        OnWindowExit += action;
    }

    private event Action OnWindowExit;


    [RelayCommand]
    private void Exit()
    {
        OnWindowExit?.Invoke();
    }
}