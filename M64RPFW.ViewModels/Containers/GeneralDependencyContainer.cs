using M64RPFW.Services;

namespace M64RPFW.ViewModels.Containers;

public class GeneralDependencyContainer
{
    public GeneralDependencyContainer(IDialogService dialogService, ILocalizationService localizationService,
        IBitmapsService bitmapsService, IDispatcherService dispatcherService, IFilesService filesService, IApplicationClosingEventService applicationClosingEventService)
    {
        DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        LocalizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        BitmapsService = bitmapsService ?? throw new ArgumentNullException(nameof(bitmapsService));
        DispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
        FilesService = filesService ?? throw new ArgumentNullException(nameof(filesService));
        ApplicationClosingEventService = applicationClosingEventService ?? throw new ArgumentNullException(nameof(applicationClosingEventService));
    }

    internal IDialogService DialogService { get; }
    internal ILocalizationService LocalizationService { get; }
    internal IBitmapsService BitmapsService { get; }
    internal IDispatcherService DispatcherService { get; }
    internal IFilesService FilesService { get; }
    internal IApplicationClosingEventService ApplicationClosingEventService { get; }
}