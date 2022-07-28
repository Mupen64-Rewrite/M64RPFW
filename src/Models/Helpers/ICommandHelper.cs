using CommunityToolkit.Mvvm.Input;

namespace M64RPFWAvalonia.Models.Helpers
{
    public static class ICommandHelper
    {
        public static void NotifyCanExecuteChanged(params IRelayCommand[] commands)
        {
            foreach (IRelayCommand cmd in commands)
                cmd.NotifyCanExecuteChanged();
        }

        public static void ExecuteIfPossible(this IRelayCommand command, object? param = null)
        {
            if (command.CanExecute(param))
            {
                command.Execute(param);
            }
        }
    }
}
