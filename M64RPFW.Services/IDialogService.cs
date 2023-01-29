namespace M64RPFW.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that displays dialogs
/// </summary>
public interface IDialogService
{
    /// <summary>
    ///     Shows an error dialog
    /// </summary>
    /// <param name="content">The dialog's content</param>
    void ShowError(string content);
}