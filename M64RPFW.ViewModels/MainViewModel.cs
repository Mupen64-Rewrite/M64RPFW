using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly GeneralDependencyContainer _generalDependencyContainer;

    public MainViewModel(GeneralDependencyContainer generalDependencyContainer)
    {
        this._generalDependencyContainer = generalDependencyContainer;

        EmulatorViewModel = new EmulatorViewModel(generalDependencyContainer);
        RecentRomsViewModel = new RecentRomsViewModel(generalDependencyContainer);
        
        _generalDependencyContainer.ApplicationClosingEventService.OnApplicationClosing += delegate
        {
            OnApplicationClosing();
        };
    }

    public EmulatorViewModel EmulatorViewModel { get; }
    public RecentRomsViewModel RecentRomsViewModel { get; }
    
    private void OnApplicationClosing()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationClosingMessage(DateTime.Now));
    }

    
}