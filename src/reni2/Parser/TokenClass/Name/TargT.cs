using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class TargT : Terminal
    {
        internal override Result Result(ContextBase context, Category category, Token token)
        {
            NotImplementedMethod(context, category, token);
            return context.CreateArgsRefResult(category);
        }

        internal override string DumpShort()
        {
            return "arg";
        }
    }
}