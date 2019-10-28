using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

namespace Reni.Context
{
    interface IRootProvider
    {
        [DisableDump]
        Root Value { get; }
    }
}