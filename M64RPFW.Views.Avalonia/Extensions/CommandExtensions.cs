using System.Windows.Input;

namespace M64RPFW.Views.Avalonia.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="ICommand" /> contract
/// </summary>
public static class CommandExtensions
{
    /// <summary>
    ///     Executs the command if its <see cref="ICommand.CanExecute" /> method returns true
    /// </summary>
    /// <param name="command">The command</param>
    /// <param name="canExecuteParameter">The object to be passed into  <see cref="ICommand.CanExecute" /></param>
    /// <param name="executeParameter">The object to be passed into  <see cref="ICommand.Execute" /> </param>
    public static void ExecuteIfPossible(this ICommand command, object? canExecuteParameter = null,
        object? executeParameter = null)
    {
        if (command.CanExecute(canExecuteParameter)) command.Execute(executeParameter);
    }
}