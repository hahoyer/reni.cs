using hw.DebugFormatter;
using hw.Parser;
using Stx.Forms;
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

        protected override IForm GetForm(Syntax parent)
        {
            var right = parent.Right;
            var left = parent.Left;
            Tracer.Assert(right == null);
            Tracer.Assert(left.TokenClass is LeftBracket);
            Tracer.Assert(left.Left == null);

            return left.Right.Form.Checked<IExpression>(parent);
        }
    }

    [BelongsTo(typeof(TokenFactory))]
    sealed class RightParentheses : TokenClass, IBracketMatch<Syntax>
    {
        public const string TokenId = ")";

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new MatchedItem();

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }
    }

    sealed class MatchedItem : ParserTokenType<Syntax>, ITokenClass
    {
        public MatchedItem(string id = "()") => Id = id;

        public override string Id {get;}

        IForm ITokenClass.GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
            => right == null ? left : Syntax.Create(left, this, token, right);
    }
}