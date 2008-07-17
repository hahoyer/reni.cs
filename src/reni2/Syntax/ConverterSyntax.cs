using Reni.Parser;
using Reni.Struct;

namespace Reni.Syntax
{
    internal sealed class ConverterSyntax : ParsedSyntax
    {
        internal readonly ICompileSyntax Body;

        internal ConverterSyntax(Token token, ICompileSyntax body) : base(token)
        {
            Body = body;
        }

        internal protected override string DumpShort()
        {
            return "converter (" + Body.DumpShort() + ")";
        }

        protected internal override IParsedSyntax SurroundedByParenthesis(Token token)
        {
            return Container.Create(token, this);
        }
    }
}