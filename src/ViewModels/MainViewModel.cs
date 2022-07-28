using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M64RPFWAvalonia.src.Models.Interaction.Interfaces;
using M64RPFWAvalonia.src.ViewModels.Interfaces;
using M64RPFWAvalonia.ViewModels;
using System.Collections.ObjectModel;
using static M64RPFWAvalonia.Models.Helpers.ICommandHelper;

namespace M64RPFWAvalonia.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public IGetVisualRoot GetVisualRoot { get; private set; }

        public EmulatorViewModel EmulatorViewModel { get; private set; }
        public RecentROMsViewModel RecentROMsViewModel { get; private set; }

        [ObservableProperty]
        private ObservableCollection<ROMViewModel> recentROMs = new();

        public MainViewModel(IGetVisualRoot getVisualRoot, ISkiaCanvasProvider skiaCanvasProvider)
        {
            this.GetVisualRoot = getVisualRoot;

            RecentROMsViewModel = new();
            EmulatorViewModel = new(RecentROMsViewModel, getVisualRoot, skiaCanvasProvider);
        }

        [RelayCommand]
        private void OnApplicationClosed()
        {
            EmulatorViewModel.CloseROMCommand.ExecuteIfPossible();
            Properties.Settings.Default.Save();
        }

    }
}
