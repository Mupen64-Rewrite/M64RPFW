﻿using System;

namespace M64RPFW.Core.Emulation.Exceptions
{
    public class ROMException : Exception
    {
        public ROMException(string message) : base(message)
        {
        }
    }
}