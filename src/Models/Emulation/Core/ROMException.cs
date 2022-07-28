using System;

namespace M64RPFWAvalonia.Models.Emulation.Core
{
    public class ROMException : Exception
    {
        public ROMException(string message) : base(message)
        {
        }
    }
}
