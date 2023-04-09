using M64RPFW.Services.Abstractions;
using static M64RPFW.Models.Types.Mupen64PlusTypes;

namespace M64RPFW.Services;

/// <summary>
/// Interface exposing custom dialogs provided by the view.
/// </summary>
public interface IViewDialogService
{
    Task ShowSettingsDialog();
    
    Task<OpenMovieDialogResult> ShowOpenMovieDialog();
}