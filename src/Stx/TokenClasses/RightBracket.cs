using hw.DebugFormatter;
using hw.Parser;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class RightBracket : TokenClass, IBracketMatch<Syntax>
    {
        public const string TokenId = "]";

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new MatchedItem();

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult(Context context, Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            Tracer.Assert(left.TokenClass is LeftBracket);
            Tracer.Assert(left.Left == null);

            return left.Right.GetResult(context.InBrackets);
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class RightParentheses : TokenClass, IBracketMatch<Syntax>
    {
        public const string TokenId = ")";

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new MatchedItem();

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult(Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(context, left, token, right);
            return null;
        }
    }

    sealed class MatchedItem : ParserTokenType<Syntax>, ITokenClass
    {
        public MatchedItem(string id = "()") => Id = id;

        public override string Id {get;}

        Result ITokenClass.GetResult(Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(context, left, token, right);
            return null;
        }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => right == null ? left : Syntax.Create(left, this, token, right);
    }
}