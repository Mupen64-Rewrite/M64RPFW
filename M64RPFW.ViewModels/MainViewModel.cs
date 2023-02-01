using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using M64RPFW.ViewModels.Containers;
using M64RPFW.ViewModels.Messages;

namespace M64RPFW.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    public MainViewModel(GeneralDependencyContainer generalDependencyContainer, SettingsViewModel settingsViewModel)
    {
        EmulatorViewModel = new EmulatorViewModel(generalDependencyContainer, settingsViewModel);
        
        generalDependencyContainer.ApplicationClosingEventService.OnApplicationClosing += OnApplicationClosing;
    }
    
    public EmulatorViewModel EmulatorViewModel { get; }
    
    private static void OnApplicationClosing()
    {
        WeakReferenceMessenger.Default.Send(new ApplicationClosingMessage(DateTime.Now));
    }

    
}