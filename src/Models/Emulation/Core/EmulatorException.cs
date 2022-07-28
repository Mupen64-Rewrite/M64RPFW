using System;

namespace M64RPFWAvalonia.Models.Emulation.Core
{
    public class EmulatorException : Exception
    {
        public EmulatorException(string? message) : base(message)
        {
        }
    }
}
