using M64RPFW.ViewModels.Interfaces;

namespace M64RPFW.ViewModels.Containers
{
    public class GeneralDependencyContainer
    {
        public GeneralDependencyContainer(IDialogProvider dialogProvider, IFileDialogProvider fileDialogProvider, IRecentRomsProvider recentRomsProvider, IRomFileExtensionsConfigurationProvider romFileExtensionsConfigurationProvider, ISavestateBoundsConfigurationProvider savestateBoundsConfigurationProvider, IThemeManager themeManager, ISettingsProvider settingsManager, ILocalizationManager localizationProvider, IDrawingSurfaceProvider drawingSurfaceProvider, IUIThreadDispatcherProvider uIThreadDispatcherProvider)
        {
            DialogProvider = dialogProvider;
            FileDialogProvider = fileDialogProvider;
            RecentRomsProvider = recentRomsProvider;
            RomFileExtensionsConfigurationProvider = romFileExtensionsConfigurationProvider;
            SavestateBoundsConfigurationProvider = savestateBoundsConfigurationProvider;
            ThemeManager = themeManager;
            SettingsManager = settingsManager;
            LocalizationProvider = localizationProvider;
            DrawingSurfaceProvider = drawingSurfaceProvider;
            UIThreadDispatcherProvider = uIThreadDispatcherProvider;
        }

        internal IDialogProvider DialogProvider { get; set; }
        internal IFileDialogProvider FileDialogProvider { get; set; }
        internal IRecentRomsProvider RecentRomsProvider { get; set; }
        internal IRomFileExtensionsConfigurationProvider RomFileExtensionsConfigurationProvider { get; set; }
        internal ISavestateBoundsConfigurationProvider SavestateBoundsConfigurationProvider { get; set; }
        internal IThemeManager ThemeManager { get; set; }
        internal ISettingsProvider SettingsManager { get; set; }
        internal ILocalizationManager LocalizationProvider { get; set; }
        internal IDrawingSurfaceProvider DrawingSurfaceProvider { get; set; }
        internal IUIThreadDispatcherProvider UIThreadDispatcherProvider { get; set; }
    }
}
