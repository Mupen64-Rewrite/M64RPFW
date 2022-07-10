using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace M64RPFW.UI.Other.Platform
{
    public static class WindowsShellWrapper
    {
        public static (string ReturnedPath, bool Cancelled) OpenFileDialogPrompt(string[] validExtensions)
        {
            var dialog = new CommonOpenFileDialog();
            string list = string.Empty;
            for (int i = 0; i < validExtensions.Length; i++)
                list += $"*.{validExtensions[i]};";
            dialog.Filters.Add(new("Supported files", list));
            dialog.EnsureFileExists = dialog.EnsurePathExists = true;
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok) return (dialog.FileName, false);
            return (string.Empty, true);

        }
        public static (string ReturnedPath, bool Cancelled) SaveFileDialogPrompt(string[] validExtensions)
        {
            var dialog = new CommonSaveFileDialog();
            string list = string.Empty;
            for (int i = 0; i < validExtensions.Length; i++)
                list += $"*.{validExtensions[i]};";
            dialog.Filters.Add(new("Supported files", list));
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok) return (dialog.FileName, false);
            return (string.Empty, true);

        }
    }
}
