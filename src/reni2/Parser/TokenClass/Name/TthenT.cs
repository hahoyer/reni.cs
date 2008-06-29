using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    class TthenT : Infix
    {
        internal override string DumpShort()
        {
            return "then";
        }

        internal override Result Result(ContextBase context, Category category, ICompileSyntax left, Token token, ICompileSyntax right)
        {
            throw new System.NotImplementedException();
        }
    }
}
