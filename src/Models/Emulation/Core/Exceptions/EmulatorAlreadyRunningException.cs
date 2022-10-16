using M64RPFW.UI.ViewModels.Interaction;
using System;

namespace M64RPFW.src.Models.Emulation.Core.Exceptions
{
    public class EmulatorAlreadyRunningException : Exception
    {
        public EmulatorAlreadyRunningException() : base()
        {
        }
    }
}
