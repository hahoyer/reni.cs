using Bnf.Forms;
using Bnf.Scanner;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.TokenClasses
{
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    [BelongsTo(typeof(TokenFactory))]
    abstract class RightParenthesis : TokenClass, IBracketMatch<Syntax>
    {
        public static string TokenId(int level) => "}])".Substring(level - 1, 1);

        public RightParenthesis(int level) => Level = level;

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new MatchedItem();

        [DisableDump]
        internal int Level {get;}

        [DisableDump]
        public override string Id => TokenId(Level);

        protected override IForm GetForm(Syntax parent)
        {
            var right = parent.Right;
            var left = parent.Left;
            Tracer.Assert(right == null);
            Tracer.Assert(left.TokenClass is LeftParenthesis);
            Tracer.Assert(left.Left == null);

            var value = left.Right;
            return value == null ? Form.Empty : value.Form.Checked<IExpression>(parent);
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