using M64RPFW.Services.Abstractions;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

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
    /// Prompts a dialog to open a movie.
    /// </summary>
    /// <param name="paramsEditable">If true, allows the user to set parameters.</param>
    /// <returns>The parameters of the movie.</returns>
    Task<OpenMovieDialogResult?> ShowOpenMovieDialog(bool paramsEditable);
}