using Avalonia.Controls;
using M64RPFWAvalonia.src.Models.Interaction.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace M64RPFWAvalonia.src.Models.Interaction.FileDialog
{
    public class FileDialog
    {
        private readonly IGetVisualRoot getVisualRoot;

        public static List<FileDialogFilter> ROMFilter { get; private set; } = new()
        {
            new()
            {
                Name = "Valid ROM Files",
                Extensions = new()
                {
                    "z64", "n64"
                }
            }
        };

        public FileDialog(IGetVisualRoot getVisualRoot)
        {
            this.getVisualRoot = getVisualRoot;
        }

        public (string ReturnedPath, bool Cancelled) SaveDialog(List<FileDialogFilter> filters)
        {
            SaveFileDialog ofd = new()
            {
                Filters = filters,
            };
            Task<string?> files = ofd.ShowAsync(getVisualRoot.GetVisualRoot());
            // short circuits
            if (files == null || files.Result == null)
                return (string.Empty, true);
            else
                return (files.Result, false);
        }

        public (string ReturnedPath, bool Cancelled) OpenDialog(List<FileDialogFilter> filters)
        {
            OpenFileDialog ofd = new()
            {
                AllowMultiple = false,
                Filters = filters,
            };
            Task<string[]?> files = ofd.ShowAsync(getVisualRoot.GetVisualRoot());
            // short circuits
            if (files == null || files.Result == null)
                return (string.Empty, true);
            else
                return (files.Result[0], false);
        }
    }
}
