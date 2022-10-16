using M64RPFW.src.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.src.Containers
{
    internal class GeneralDependencyContainer
    {
        public GeneralDependencyContainer(IDialogProvider dialogProvider, IFileDialogProvider fileDialogProvider, IRecentRomsProvider recentRomsProvider, IRomFileExtensionsConfigurationProvider romFileExtensionsConfigurationProvider, ISavestateBoundsConfigurationProvider savestateBoundsConfigurationProvider, IThemeManager themeManager, IWindowClosingProvider windowClosingProvider)
        {
            DialogProvider = dialogProvider;
            FileDialogProvider = fileDialogProvider;
            RecentRomsProvider = recentRomsProvider;
            RomFileExtensionsConfigurationProvider = romFileExtensionsConfigurationProvider;
            SavestateBoundsConfigurationProvider = savestateBoundsConfigurationProvider;
            ThemeManager = themeManager;
            WindowClosingProvider = windowClosingProvider;
        }

        internal IDialogProvider DialogProvider { get; set; }
        internal IFileDialogProvider FileDialogProvider { get; set; }
        internal IRecentRomsProvider RecentRomsProvider { get; set; }
        internal IRomFileExtensionsConfigurationProvider RomFileExtensionsConfigurationProvider { get; set; }
        internal ISavestateBoundsConfigurationProvider SavestateBoundsConfigurationProvider { get; set; }
        internal IThemeManager ThemeManager { get; set; }
        internal IWindowClosingProvider WindowClosingProvider { get; set; }

    }
}
