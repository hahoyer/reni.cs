using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser
{
    internal class ExpressionSyntax : CompileSyntax
    {
        internal readonly ICompileSyntax Left;
        internal readonly ICompileSyntax Right;
        private readonly DefineableToken DefineableToken;

        internal ExpressionSyntax(ICompileSyntax left, Token token, ICompileSyntax right)
            : base(token)
        {
            Left = left;
            DefineableToken = new DefineableToken(token);
            Right = right;
        }

        internal protected override string DumpShort()
        {
            var result = DefineableToken.Name;
            if(Left != null)
                result = "(" + Left.DumpShort() + ")" + result;
            if(Right != null)
                result += "(" + Right.DumpShort() + ")";
            return result;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            if(Left == null)
                return PrefixResult(context, category);
            return InfixResult(context, category);
        }

        private Result PrefixResult(ContextBase context, Category category)
        {
            var contextSearchResult = context.SearchDefineable(DefineableToken);
            if(contextSearchResult.IsSuccessFull)
                return contextSearchResult.Feature.ApplyResult(context, category, Right);

            if(Right == null)
            {
                NotImplementedMethod(context, category, "contextSearchResult", contextSearchResult);
                return null;
            }

            var argResult = context.Result(category | Category.Type, Right);
            var prefixSearchResult = argResult.Type.SearchDefineablePrefix(DefineableToken);
            if(prefixSearchResult.IsSuccessFull)
                return prefixSearchResult.Feature.Result(category, argResult);

            NotImplementedMethod(context, category, "contextSearchResult", contextSearchResult, "prefixSearchResult", prefixSearchResult);
            return null;
        }

        private Result InfixResult(ContextBase context, Category category)
        {
            var leftType = context.Type(Left);
            var searchResult = leftType.SearchDefineable(DefineableToken);
            if(searchResult.IsSuccessFull)
                return searchResult.Feature.ApplyResult(context, category, Left, Right);
            NotImplementedMethod(context, category, "leftType", leftType, "searchResult", searchResult);
            return null;
        }
    }
}