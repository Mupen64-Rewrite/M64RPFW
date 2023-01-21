using M64RPFW.Services;

namespace M64RPFW.ViewModels.Containers;

public class GeneralDependencyContainer
{
    public GeneralDependencyContainer(IDialogService dialogService,
        ISettingsService settingsService, ILocalizationService localizationService,
        IBitmapDrawingService bitmapDrawingService, IDispatcherService dispatcherService, IFilesService filesService, IApplicationClosingEventService applicationClosingEventService)
    {
        DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        SettingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        LocalizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        BitmapDrawingService = bitmapDrawingService ?? throw new ArgumentNullException(nameof(bitmapDrawingService));
        DispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
        FilesService = filesService ?? throw new ArgumentNullException(nameof(filesService));
        ApplicationClosingEventService = applicationClosingEventService ?? throw new ArgumentNullException(nameof(applicationClosingEventService));
    }

    internal IDialogService DialogService { get; }
    internal ISettingsService SettingsService { get; }
    internal ILocalizationService LocalizationService { get; }
    internal IBitmapDrawingService BitmapDrawingService { get; }
    internal IDispatcherService DispatcherService { get; }
    internal IFilesService FilesService { get; }
    internal IApplicationClosingEventService ApplicationClosingEventService { get; }
}