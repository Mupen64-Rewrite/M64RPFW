using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using Mupen64PlusRR.Views;

namespace Mupen64PlusRR.Services;

/// <summary>
///     A <see langword="class" /> that implements the <see cref="IFilesService" /> <see langword="interface" /> using
///     Avalonia APIs
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
        return new Abstractions.File(path);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<IFile> CreateOrOpenFileFromPathAsync(string path)
    {
        var folderPath = Path.GetDirectoryName(path);
        var filename = Path.GetFileName(path);

        if (folderPath != null && !Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        if (!System.IO.File.Exists(path)) System.IO.File.Create(path);

        return new Abstractions.File(path);
    }
    
    private static Window GetWindow()
    {
        var lifetime = Avalonia.Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return lifetime?.MainWindow!;
    }
    
    /// <inheritdoc />
    public async Task<IFile?> TryPickOpenFileAsync(string[] extensions)
    {
        var fileDialog = new OpenFileDialog
        {
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Extensions = extensions.ToList()
                }
            }
        };
        var result = await fileDialog.ShowAsync(GetWindow());

        return result is { Length: > 0 } ? new Abstractions.File(result[0]) : null;
    }

    /// <inheritdoc />
    public async Task<IFile?> TryPickSaveFileAsync(string filename, (string Name, string[] Extensions) fileType)
    {
        var fileDialog = new SaveFileDialog
        {
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Extensions = fileType.Extensions.ToList(),
                    Name = fileType.Name
                }
            }
        };
        var result = await fileDialog.ShowAsync(GetWindow());

        return result != null ? new Abstractions.File(result) : null;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<(IFile, string)> GetFutureAccessFilesAsync()
    {
        yield return await Task.FromResult<(IFile, string)>((null, null));
        throw new NotImplementedException();
    }

    public Task<bool> IsAccessible(string path)
    {
        try
        {
            using (var file = File.OpenRead(path))
            {
            }

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}