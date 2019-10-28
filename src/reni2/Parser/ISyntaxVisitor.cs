using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Parser
{
    interface ISyntaxVisitor
    {
        Value Arg { get; }
    }
}