﻿using CommunityToolkit.Mvvm.Input;

namespace M64RPFW.ViewModels.Helpers
{
    public static class ICommandHelper
    {
        public static void NotifyCanExecuteChanged(params IRelayCommand[] commands)
        {
            foreach (IRelayCommand cmd in commands)
            {
                cmd.NotifyCanExecuteChanged();
            }
        }

        public static void ExecuteIfPossible(this IRelayCommand command, object? parameter = null)
        {
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}
