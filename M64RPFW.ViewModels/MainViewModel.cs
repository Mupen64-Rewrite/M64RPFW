using CommunityToolkit.Mvvm.ComponentModel;
using M64RPFW.Services;

namespace M64RPFW.ViewModels;

public class MainViewModel : ObservableObject
{
    public EmulatorViewModel EmulatorViewModel { get; }


    public MainViewModel(IDispatcherService dispatcherService, IFilePickerService filePickerService,
        IOpenGLContextService openGlContextService, IWindowAccessService windowAccessService,
        IViewDialogService viewDialogService)
    {
        EmulatorViewModel = new EmulatorViewModel(openGlContextService, dispatcherService, filePickerService,
            windowAccessService, viewDialogService);
    }
}