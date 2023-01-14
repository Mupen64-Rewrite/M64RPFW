namespace M64RPFW.Models.Emulation.Exceptions;

public class PluginAlreadyAttachedException : Exception
{
    public PluginAlreadyAttachedException(string? message) : base(message)
    {
    }
}