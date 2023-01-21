using CommunityToolkit.Mvvm.Messaging.Messages;

namespace M64RPFW.ViewModels.Messages;

public class ApplicationClosingMessage : ValueChangedMessage<DateTime>
{
    public ApplicationClosingMessage(DateTime value) : base(value)
    {
    }
}