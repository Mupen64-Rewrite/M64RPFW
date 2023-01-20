using CommunityToolkit.Mvvm.Messaging.Messages;

namespace M64RPFW.ViewModels.Messages;

public class ApplicationExitingMessage : ValueChangedMessage<DateTime>
{
    public ApplicationExitingMessage(DateTime value) : base(value)
    {
    }
}