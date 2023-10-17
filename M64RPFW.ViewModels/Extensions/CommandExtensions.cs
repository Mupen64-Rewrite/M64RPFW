using System.Windows.Input;

namespace M64RPFW.ViewModels.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="ICommand" /> contract
/// </summary>
public static class CommandExtensions
{
    /// <summary>
    /// Executes the command if its <see cref="ICommand.CanExecute" /> method returns true
    /// </summary>
    /// <param name="command">The command</param>
    /// <param name="commandParam">The paramter to pass to the command </param>
    public static void ExecuteIfPossible(this ICommand command, object? commandParam = null)
    {
        if (command.CanExecute(commandParam)) command.Execute(commandParam);
    }
}