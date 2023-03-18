using CommunityToolkit.Mvvm.Messaging.Messages;

namespace M64RPFW.ViewModels.Messages;

internal class RomLoadingMessage : ValueChangedMessage<string>
{
    public RomLoadingMessage(string value) : base(value)
    {
    }
}