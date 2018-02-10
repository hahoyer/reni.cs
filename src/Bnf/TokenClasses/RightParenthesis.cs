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
    sealed class RightParenthesis : TokenType, IBracketMatch<Syntax>
    {
        public static string TokenId(int level) => "}])".Substring(level - 1, length: 1);

        public RightParenthesis(int level) => Level = level;

        IPriorityParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new MatchedItem();

        [DisableDump]
        internal int Level {get;}

        [DisableDump]
        public override string Id => TokenId(Level);

        protected override IForm GetForm(Syntax parent)
        {
            var right = parent.Right;
            var left = parent.Left;
            Tracer.Assert(right == null);
            Tracer.Assert(left.TokenType is LeftParenthesis);
            Tracer.Assert(left.Left == null);
            Tracer.Assert(left.Right != null);

            var result = (IExpression) left.Right.Form.Checked<IExpression>(parent);
            switch(Level)
            {
                case 1: return new Repeat(parent, result);
                case 2: return new Option(parent, result);
                default: return result;
            }
        }
    }
}