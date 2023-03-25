using M64RPFW.Services.Abstractions;

// original: https://github.com/Sergio0694/Brainf_ckSharp/blob/master/src/Brainf_ckSharp.Services/IFilesService.cs
// extended by aurumaker72

namespace M64RPFW.Services;

/// <summary>
///     Interface for a service exposing a file picker from the view.
/// </summary>
public interface IFilePickerService
{

    /// <summary>
    /// Shows a file picker to open a file with the specified options.
    /// </summary>
    /// <param name="title">The dialog's title.</param>
    /// <param name="options">The options that may be used.</param>
    /// <param name="allowMultiple">If true, allow multiple files to be selected.</param>
    /// <returns>An <see cref="IFile" /> to open, if available</returns>
    Task<string[]?> ShowOpenFilePickerAsync(string title = "Open file...",
        IReadOnlyList<FilePickerOption>? options = null, bool allowMultiple = false);

    /// <summary>
    /// Shows a file picker to open a file with the specified options.
    /// </summary>
    /// <param name="title">The dialog's title.</param>
    /// <param name="options">The options that may be used.</param>
    /// <returns>An <see cref="IFile" /> to open, if available</returns>
    Task<string?> ShowSaveFilePickerAsync(string title = "Save file...",
        IReadOnlyList<FilePickerOption>? options = null);
}