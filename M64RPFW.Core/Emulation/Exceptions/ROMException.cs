using System;

namespace M64RPFW.Models.Emulation.Exceptions
{
    public class ROMException : Exception
    {
        public ROMException(string message) : base(message)
        {
        }
    }
}
