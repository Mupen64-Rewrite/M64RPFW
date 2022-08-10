using System;
using CommunityToolkit.Mvvm.Input;
using Eto.Forms;

namespace M64RPFW.Misc;

public class ForwardingRelayCommand : Command
{
    public ForwardingRelayCommand(IRelayCommand command)
    {
        _command = command;
        Executed += (_, _) => _command.Execute(null);
        _command.CanExecuteChanged += (_, _) => OnEnabledChanged(EventArgs.Empty);
    }

    public override bool Enabled => _command.CanExecute(null);

    private IRelayCommand _command;
}