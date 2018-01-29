using hw.DebugFormatter;
using hw.Parser;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Case : TokenClass
    {
        public const string TokenId = "CASE";

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult
            (Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class EndCase : TokenClass, IBracketMatch<Syntax>
    {
        public const string TokenId = "END_CASE";

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new MatchedItem();

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult
            (Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}