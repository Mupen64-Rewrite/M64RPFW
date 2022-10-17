using System;

namespace M64RPFW.src.Models.Emulation.Core.Exceptions
{
    public class PluginAttachException : Exception
    {
        public PluginAttachException(string? message) : base(message)
        {
        }
    }
}
