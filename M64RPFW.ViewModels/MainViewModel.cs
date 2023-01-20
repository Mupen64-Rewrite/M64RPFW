using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    public MainViewModel(GeneralDependencyContainer generalDependencyContainer)
    {
        this._generalDependencyContainer = generalDependencyContainer;

        EmulatorViewModel = new EmulatorViewModel(generalDependencyContainer);
        RecentRomsViewModel = new RecentRomsViewModel(generalDependencyContainer);
    }

    public EmulatorViewModel EmulatorViewModel { get; }
    public RecentRomsViewModel RecentRomsViewModel { get; }
    
    [RelayCommand]
    private void Exit()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationExitingMessage(DateTime.Now));
    }
}