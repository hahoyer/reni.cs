using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Formatting
{
    internal interface ILineLengthLimiter
    {
        int MaxLineLength { get; }
    }
}