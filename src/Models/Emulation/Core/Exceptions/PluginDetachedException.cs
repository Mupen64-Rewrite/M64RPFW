using M64RPFW.UI.ViewModels.Interaction;
using System;

namespace M64RPFW.src.Models.Emulation.Core.Exceptions
{
    public class PluginDetachedException : Exception
    {
        public PluginDetachedException(string? message) : base(message)
        {
        }
    }
}
