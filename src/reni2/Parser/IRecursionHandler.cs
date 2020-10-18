using Reni.Basics;
using Reni.Context;
using Reni.SyntaxTree;

namespace Reni.Parser
{
    interface IRecursionHandler
    {
        Result Execute
            (
            ContextBase context,
            Category category,
            Category pendingCategory,
            ValueSyntax syntax,
            bool asReference);
    }
}