using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class TthisT : Terminal
    {
        internal override string DumpShort()
        {
            return "this";
        }

        internal override Result Result(ContextBase context, Category category, Token token)
        {
            return context.CreateThisRefResult(category);
        }
    }
}