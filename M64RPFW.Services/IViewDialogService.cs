using M64RPFW.Services.Abstractions;

namespace M64RPFW.Services;

/// <summary>
/// Interface exposing custom dialogs provided by the view.
/// </summary>
public interface IViewDialogService
{
    /// <summary>
    /// Shows the settings dialog.
    /// </summary>
    Task ShowSettingsDialog();

    /// <summary>
    /// Shows the advanced settings dialog.
    /// </summary>
    Task ShowAdvancedSettingsDialog();

    /// <summary>
    /// Prompts a dialog to open a movie.
    /// </summary>
    /// <param name="paramsEditable">If true, allows the user to set parameters.</param>
    /// <returns>The parameters of the movie.</returns>
    Task<OpenMovieDialogResult?> ShowOpenMovieDialog(bool paramsEditable);

    /// <summary>
    /// Shows a dialog with a message and exception stack trace.
    /// </summary>
    /// <param name="e">The exception in question</param>
    /// <param name="msg">The message</param>
    Task ShowExceptionDialog(Exception e, string? msg = null);
}