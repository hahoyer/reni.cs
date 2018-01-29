using hw.DebugFormatter;
using hw.Parser;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class LeftBracket : TokenClass
    {
        public const string TokenId = "[";

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult(Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right, context);
            return null;
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class LeftParentheses : TokenClass
    {
        public const string TokenId = "(";

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult(Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right, context);
            return null;
        }
    }
}