using M64RPFW.Services.Abstractions;

// original: https://github.com/Sergio0694/Brainf_ckSharp/blob/master/src/Brainf_ckSharp.Services/IFilesService.cs
// extended by aurumaker72

namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that handles files
/// </summary>
public interface IFilesService
{
    /// <summary>
    ///     Gets the path of the installation directory
    /// </summary>
    string InstallationPath { get; }

    /// <summary>
    ///     Gets the path of the local temporary files folder
    /// </summary>
    string TemporaryFilesPath { get; }

    /// <summary>
    ///     Gets a target file from a specified path
    /// </summary>
    /// <param name="path">The path of the file to retrieve</param>
    /// <returns>The file retrieved from the specified path</returns>
    Task<IFile> GetFileFromPathAsync(string path);

    /// <summary>
    ///     Tries to get a target file from a specified path
    /// </summary>
    /// <param name="path">The path of the file to retrieve</param>
    /// <returns>The file retrieved from the specified path, if existing</returns>
    Task<IFile?> TryGetFileFromPathAsync(string path);

    /// <summary>
    ///     Tries to open or create a file from a specified path
    /// </summary>
    /// <param name="path">The path of the file to create or open</param>
    /// <returns>The file created or opened from the specified path</returns>
    Task<IFile> CreateOrOpenFileFromPathAsync(string path);

    /// <summary>
    /// Shows a file picker to open a file with the specified options.
    /// </summary>
    /// <param name="title">The dialog's title.</param>
    /// <param name="options">The options that may be used.</param>
    /// <param name="allowMultiple">If true, allow multiple files to be selected.</param>
    /// <returns>An <see cref="IFile" /> to open, if available</returns>
    Task<IFile[]?> ShowOpenFilePickerAsync(string title = "Open file...",
        IReadOnlyList<FilePickerOption>? options = null, bool allowMultiple = false);

    /// <summary>
    /// Shows a file picker to open a file with the specified options.
    /// </summary>
    /// <param name="title">The dialog's title.</param>
    /// <param name="options">The options that may be used.</param>
    /// <returns>An <see cref="IFile" /> to open, if available</returns>
    Task<IFile?> ShowSaveFilePickerAsync(string title = "Open file...",
        IReadOnlyList<FilePickerOption>? options = null);

    /// <summary>
    ///     Enumerates all the available files from the future access list.
    /// </summary>
    /// <returns>An <see cref="IAsyncEnumerable{T}" /> sequence of available files.</returns>
    IAsyncEnumerable<(IFile File, string Metadata)> GetFutureAccessFilesAsync();

    /// <summary>
    ///     Checks whether the file at the specified path is accessible
    /// </summary>
    /// <param name="path">The path of the file to retrieve</param>
    /// <returns>Whether the file is accessible</returns>
    Task<bool> IsAccessible(string path);
}