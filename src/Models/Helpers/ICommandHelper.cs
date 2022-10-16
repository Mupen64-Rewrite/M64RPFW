using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace M64RPFW.Models.Helpers
{
    public static class ICommandHelper
    {
        public static void NotifyCanExecuteChanged(params IRelayCommand[] commands)
        {
            foreach (IRelayCommand cmd in commands)
                cmd.NotifyCanExecuteChanged();
        }
    }
}
