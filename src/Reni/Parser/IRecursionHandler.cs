using Reni.Basics;
using Reni.Context;
using Reni.SyntaxTree;

namespace Reni.Parser;

[Obsolete("",true)]
interface IRecursionHandler
{
    Result Execute(ContextBase context, Category category, ValueSyntax syntax, bool asReference);
}