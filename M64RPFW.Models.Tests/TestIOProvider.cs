using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.Models.Tests
{
    internal static class TestIOProvider
    {
        internal static string ToBundledPath(this string fileName) => Path.Combine("Bundled", fileName);
    }
}
