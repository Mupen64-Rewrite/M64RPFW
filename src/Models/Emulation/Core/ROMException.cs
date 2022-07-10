using System;

namespace M64RPFW.Models.Emulation.Core
{
    public class ROMException : Exception
    {
        public ROMException(string message) : base(message)
        {
        }
    }
}
