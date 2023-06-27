using CommunityToolkit.Mvvm.Messaging.Messages;

namespace M64RPFW.ViewModels.Messages;

public class LuaLoadingMessage : ValueChangedMessage<string>
{
    public LuaLoadingMessage(string value) : base(value)
    {
    }
}