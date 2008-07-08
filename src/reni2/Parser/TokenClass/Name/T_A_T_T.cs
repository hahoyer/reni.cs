using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class T_A_T_T : Infix
    {
        internal override string DumpShort()
        {
            return "_A_T_";
        }

        internal override Result Result(ContextBase context, Category category, ICompileSyntax left, Token token, ICompileSyntax right)
        {
            var objectResult = context.ResultAsRef(category|Category.Type, left);
            var index = context.Evaluate(right, context.Type(left).IndexType).ToInt32();
            return objectResult.Type.AccessResult(category, index).UseWithArg(objectResult);
        }
    }
}