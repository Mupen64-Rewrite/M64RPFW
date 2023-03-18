using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using M64RPFW.Views.Avalonia.Services.Abstractions;
using File = System.IO.File;

namespace M64RPFW.Views.Avalonia.Services;

/// <summary>
///     A <see langword="class" /> that implements the <see cref="IFilesService" /> <see langword="interface" /> using
///     <see cref="IStorageProvider"/> and <see cref="IStorageFile"/>
/// </summary>
public sealed class FilesService : IFilesService
{
    /// <inheritdoc />
    public string InstallationPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    /// <inheritdoc />
    public string TemporaryFilesPath => Path.GetTempPath();

    /// <inheritdoc />
    public async Task<IFile> GetFileFromPathAsync(string path)
    {
        var file = await TryGetFileFromPathAsync(path);
        if (file == null)
            throw new FileNotFoundException($"File not found: {path}");
        return file;
    }

    /// <inheritdoc />
    public async Task<IFile?> TryGetFileFromPathAsync(string path)
    {
        var provider = GetWindow().StorageProvider;
        var storageFile = await provider.TryGetFileFromPath(path);
        return storageFile == null ? null : new StorageFileWrapper(storageFile);
    }

    /// <inheritdoc />
    public async Task<IFile> CreateOrOpenFileFromPathAsync(string path)
    {
        var provider = GetWindow().StorageProvider;
        var storageFile = await provider.TryGetFileFromPath(path);
        if (storageFile != null)
            return new StorageFileWrapper(storageFile);
        // Try to create the file via System.IO
        await File.Create(path).DisposeAsync();
        // Access the file again
        storageFile = await provider.TryGetFileFromPath(path);
        if (storageFile == null)
            throw new FileNotFoundException($"File not found: {path}");
        return new StorageFileWrapper(storageFile);
    }
    
    private static Window GetWindow()
    {
        var lifetime = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return lifetime?.MainWindow!;
    }

    public async Task<IFile[]?> ShowOpenFilePickerAsync(string title = "Open file...", IReadOnlyList<FilePickerOption>? options = null, bool allowMultiple = false)
    {
        var provider = GetWindow().StorageProvider;
        var storageFiles = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            FileTypeFilter = options?.Select(FilePickerTypeExtensions.ToFilePickerFileType).ToArray(),
            AllowMultiple = allowMultiple
        });
        return storageFiles.Select(sf => (IFile) new StorageFileWrapper(sf)).ToArray();
    }

    public async Task<IFile?> ShowSaveFilePickerAsync(string title = "Open file...", IReadOnlyList<FilePickerOption>? options = null)
    {
        var provider = GetWindow().StorageProvider;
        var storageFile = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            FileTypeChoices = options?.Select(FilePickerTypeExtensions.ToFilePickerFileType).ToArray()
        });
        return storageFile != null ? new StorageFileWrapper(storageFile) : null;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<(IFile, string)> GetFutureAccessFilesAsync()
    {
        yield return await Task.FromResult<(IFile, string)>((null!, null!));
        throw new NotImplementedException();
    }

    public async Task<bool> IsAccessible(string path)
    {
        var provider = GetWindow().StorageProvider;
        var storageFile = await provider.TryGetFileFromPath(path);
        return storageFile != null && storageFile.CanOpenRead;
    }
}