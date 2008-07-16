using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    [Token("function")]
    internal sealed class TfunctionT : Prefix
    {
        internal override Result Result(ContextBase context, Category category, Token token, ICompileSyntax right)
        {
            return context.CreateFunctionResult(category, right);
        }

        internal override string DumpShort()
        {
            return "function";
        }
    }
}