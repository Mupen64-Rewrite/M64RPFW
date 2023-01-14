using CommunityToolkit.Mvvm.Messaging.Messages;

namespace M64RPFW.ViewModels.Messages;

public class RomLoadedMessage : ValueChangedMessage<RomViewModel>
{
    public RomLoadedMessage(RomViewModel value) : base(value)
    {
    }
}