using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.Models.Emulation.Exceptions
{
    internal class UnresolvableConfigEntryTypeException : Exception
    {
        public UnresolvableConfigEntryTypeException(string? message) : base(message)
        {
        }
    }
}
