using System.Collections.Generic;
using Reni.Code;

namespace Reni.Context
{
    internal interface IRootContext
    {
        CodeBase[] FunctionCode { get; }
        FunctionList Functions { get; }
        List<Container> CompileFunctions();
    }
}