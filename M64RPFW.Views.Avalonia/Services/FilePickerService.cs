using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Helpers;
using M64RPFW.Views.Avalonia.Services.Abstractions;

namespace M64RPFW.Views.Avalonia.Services;

/// <summary>
///     A <see langword="class" /> that implements the <see cref="IFilePickerService" /> <see langword="interface" /> using
///     <see cref="IStorageProvider"/> and <see cref="IStorageFile"/>
/// </summary>
public sealed class FilePickerService : IFilePickerService
{
    private FilePickerService() {}

    public static FilePickerService Instance { get; } = new();

    public async Task<string[]?> ShowOpenFilePickerAsync(string title = "Open file...", IReadOnlyList<FilePickerOption>? options = null, bool allowMultiple = false)
    {
        var provider = WindowHelper.GetFirstActiveWindow().StorageProvider;
        var storageFiles = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            FileTypeFilter = options?.Select(FilePickerTypeExtensions.ToFilePickerFileType).ToArray(),
            AllowMultiple = allowMultiple
        });
        return storageFiles.Count != 0? storageFiles.Select(sf => sf.Path.LocalPath).ToArray() : null;
    }

    public async Task<string?> ShowSaveFilePickerAsync(string title = "Open file...", IReadOnlyList<FilePickerOption>? options = null)
    {
        var provider = WindowHelper.GetFirstActiveWindow().StorageProvider;
        var storageFile = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            FileTypeChoices = options?.Select(FilePickerTypeExtensions.ToFilePickerFileType).ToArray()
        });
        return storageFile?.Path.LocalPath;
    }
}