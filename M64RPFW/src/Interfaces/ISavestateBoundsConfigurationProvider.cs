﻿using M64RPFW.src.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M64RPFW.src.Interfaces
{
    internal interface ISavestateBoundsConfigurationProvider
    {
        internal SavestateBoundsConfiguration SavestateBoundsConfiguration { get; }
    }
}