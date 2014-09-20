using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Feature
{
    interface IConversionParameter
    {
        bool EnableCut { get; }
        IConversionParameter EnsureEnableCut { get; }
    }
}