using System;
using CommunityToolkit.Mvvm.Input;
using Eto.Forms;

namespace M64RPFW.Misc;

/// <summary>
/// A <see cref="Command"/> wrapping an <see cref="IRelayCommand"/>.
/// Made to ease writing something closer to MVVM-style commands.
/// </summary>
public class RelayWrapperCommand : Command
{
    public RelayWrapperCommand(IRelayCommand command)
    {
        _command = command;
        Executed += (_, _) => _command.Execute(null);
        _command.CanExecuteChanged += (_, _) => OnEnabledChanged(EventArgs.Empty);
    }

    public override bool Enabled => _command.CanExecute(null);

    private readonly IRelayCommand _command;
}