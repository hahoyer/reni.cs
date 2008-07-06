using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    internal sealed class DefinableTokenSyntax : CompileSyntax
    {
        private readonly DefineableToken _defineableToken;

        public DefinableTokenSyntax(Token token) : base(token)
        {
            _defineableToken = new DefineableToken(token);
        }

        internal protected override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            return new DeclarationSyntax(_defineableToken, token, right);
        }

        [DumpData(false)]
        internal protected override ICompileSyntax ToCompileSyntax { get { return this; } }

        internal protected override Result Result(ContextBase context, Category category)
        {
            var contextSearchResult = context.SearchDefineable(_defineableToken);
            if(contextSearchResult.IsSuccessFull)
                return contextSearchResult.Feature.ApplyResult(context, category, null);

            NotImplementedMethod(context, category, "contextSearchResult", contextSearchResult);
            return null;
        }
    }
}