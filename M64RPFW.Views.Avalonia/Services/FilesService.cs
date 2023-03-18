using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using M64RPFW.Services;
using M64RPFW.Services.Abstractions;
using File = M64RPFW.Views.Avalonia.Services.Abstractions.File;

namespace M64RPFW.Views.Avalonia.Services;

/// <summary>
///     A <see langword="class" /> that implements the <see cref="IFilesService" /> <see langword="interface" /> using
///     Avalonia APIs
///     APIs
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
        return new File(path);
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

        return new File(path);
    }

    /// <inheritdoc />
    public async Task<IFile?> TryPickOpenFileAsync(string[] extensions)
    {
        OpenFileDialog ofd = new()
        {
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Name = "Allowed files",
                    Extensions = extensions.ToList()
                }
            },
            AllowMultiple = false
        };
        var paths = await ofd.ShowAsync(GetWindow());
        if (paths == null || paths.Length == 0) return null;
        return new File(paths[0]);
    }

    /// <inheritdoc />
    public async Task<IFile?> TryPickSaveFileAsync(string filename, (string Name, string[] Extensions) fileType)
    {
        SaveFileDialog sfd = new()
        {
            Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Name = fileType.Name,
                    Extensions = fileType.Extensions.ToList()
                }
            }
        };
        var paths = await sfd.ShowAsync(GetWindow());
        return paths == null ? null : new File(paths);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<(IFile, string)> GetFutureAccessFilesAsync()
    {
        yield return await Task.FromResult<(IFile, string)>((null, null));
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<bool> IsAccessible(string path)
    {
        try
        {
            await using (_ = System.IO.File.Open(path, FileMode.Open))
            {
                ;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Window GetWindow()
    {
        var lifetime = Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return lifetime?.MainWindow!;
    }
}