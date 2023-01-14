using M64RPFW.Services;
using M64RPFW.ViewModels.Configurations;

namespace M64RPFW.ViewModels.Containers;

public class GeneralDependencyContainer
{
    public GeneralDependencyContainer(IDialogService dialogService, IThemeService themeService,
        ISettingsService settingsService, ILocalizationService localizationService,
        IBitmapDrawingService bitmapDrawingService, IDispatcherService dispatcherService, IFilesService filesService)
    {
        DialogService = dialogService;
        ThemeService = themeService;
        SettingsService = settingsService;
        LocalizationService = localizationService;
        BitmapDrawingService = bitmapDrawingService;
        DispatcherService = dispatcherService;
        FilesService = filesService;
    }

    internal IDialogService DialogService { get; }
    internal IThemeService ThemeService { get; }
    internal ISettingsService SettingsService { get; }
    internal ILocalizationService LocalizationService { get; }
    internal IBitmapDrawingService BitmapDrawingService { get; }
    internal IDispatcherService DispatcherService { get; }
    internal IFilesService FilesService { get; }

    internal FileExtensionsConfiguration RomFileExtensionsConfiguration { get; } = new();
    internal SavestateConfiguration SavestateBoundsConfiguration { get; } = new();
}