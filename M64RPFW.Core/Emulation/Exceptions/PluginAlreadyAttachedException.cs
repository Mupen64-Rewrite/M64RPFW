using System;

namespace M64RPFW.src.Models.Emulation.Core.Exceptions
{
    public class PluginAlreadyAttachedException : Exception
    {
        public PluginAlreadyAttachedException(string? message) : base(message)
        {
        }
    }
}
