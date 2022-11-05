using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.src.Services
{
    /// <summary>
    /// A <see langword="class"/> that implements the <see cref="IFilesService"/> <see langword="interface"/> using WPF APIs
    /// </summary>
    public sealed class FilesService : IFilesService
    {
        /// <inheritdoc/>
        public string InstallationPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        /// <inheritdoc/>
        public string TemporaryFilesPath => Path.GetTempPath();

        /// <inheritdoc/>
        public async Task<IFile> GetFileFromPathAsync(string path)
        {
            return new Abstractions.File(path);
        }

        /// <inheritdoc/>
        public async Task<IFile?> TryGetFileFromPathAsync(string path)
        {
            try
            {
                return await GetFileFromPathAsync(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IFile> CreateOrOpenFileFromPathAsync(string path)
        {
            string? folderPath = Path.GetDirectoryName(path);
            string filename = Path.GetFileName(path);

            if (folderPath != null && !Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(path))
            {
                File.Create(path);
            }

            return new Abstractions.File(path);
        }

        /// <inheritdoc/>
        public async Task<IFile?> TryPickOpenFileAsync(string[] extensions)
        {
            CommonOpenFileDialog dialog = new();
            string list = string.Empty;
            for (int i = 0; i < extensions.Length; i++)
            {
                list += $"*.{extensions[i]};";
            }

            dialog.Filters.Add(new("Supported formats", list));
            dialog.EnsureFileExists = dialog.EnsurePathExists = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return new Abstractions.File(dialog.FileName);
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<IFile?> TryPickSaveFileAsync(string filename, (string Name, string[] Extensions) fileType)
        {
            CommonSaveFileDialog dialog = new();
            string list = string.Empty;
            for (int i = 0; i < fileType.Extensions.Length; i++)
            {
                list += $"*.{fileType.Extensions[i]};";
            }

            dialog.Filters.Add(new(fileType.Name, list));
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                return new Abstractions.File(dialog.FileName);
            }
            return null;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<(IFile, string)> GetFutureAccessFilesAsync()
        {
            yield return await Task.FromResult<(IFile, string)>((null, null));
            throw new NotImplementedException();
        }
    }
}