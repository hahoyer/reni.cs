using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    internal sealed class DefinableTokenSyntax : ParsedSyntax, ICompileSyntax
    {
        private readonly DefineableToken _defineableToken;

        public DefinableTokenSyntax(Token token) : base(token)
        {
            _defineableToken = new DefineableToken(token);
        }

        protected internal override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right)
        {
            return new DeclarationSyntax(_defineableToken, token, right);
        }

        Result ICompileSyntax.Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context,category);
            return null;
        }

        string ICompileSyntax.DumpShort()
        {
            return DumpShort();
        }

        string ICompileSyntax.FilePosition()
        {
            return FilePosition();
        }
    }
}