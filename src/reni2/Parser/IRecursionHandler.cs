using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;

namespace Reni.Parser
{
    interface IRecursionHandler
    {
        Result Execute
            (
            ContextBase context,
            Category category,
            Category pendingCategory,
            Value syntax,
            bool asReference);
    }
}