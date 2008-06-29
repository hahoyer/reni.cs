using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    /// <summary>
    /// Summary description for functionToken.
    /// </summary>
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

    /// <summary>
    /// Summary description for functionToken.
    /// </summary>
    internal sealed class TpropertyT : Prefix
    {
        internal override Result Result(ContextBase context, Category category, Token token, ICompileSyntax right)
        {
            return context.CreatePropertyResult(category, right);
        }

        internal override string DumpShort()
        {
            return "property";
        }
    }
}