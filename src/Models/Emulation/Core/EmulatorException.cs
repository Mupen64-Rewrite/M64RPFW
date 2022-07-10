using M64RPFW.UI.ViewModels.Interaction;
using System;

namespace M64RPFW.Models.Emulation.Core
{
    public class EmulatorException : Exception
    {
        public readonly StatusInformation? AdditionalInformation;

        public EmulatorException(StatusInformation additionalInformation = null)
        {
            this.AdditionalInformation = additionalInformation;
        }
    }
}
