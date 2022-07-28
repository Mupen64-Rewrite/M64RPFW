using Avalonia.Controls;
using Avalonia.VisualTree;
using M64RPFWAvalonia.src.Controls;
using M64RPFWAvalonia.src.Models.Interaction.Interfaces;
using M64RPFWAvalonia.UI.ViewModels;

namespace M64RPFW_Avalonia
{
    public partial class MainWindow : Window, IGetVisualRoot
    {
        private readonly MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new(this, this.FindControl<SkiaCanvas>("MainCanvas"));
            this.DataContext = mainViewModel;
        }

        public Window GetVisualRoot()
        {
            return (Window)this.PrimaryWindow.GetVisualRoot();
        }
    }
}