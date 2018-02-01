using hw.DebugFormatter;
using hw.Parser;
using Stx.Forms;
using Stx.Scanner;

namespace Stx.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Case : TokenClass
    {
        public const string TokenId = "CASE";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
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

        protected override IForm GetForm(Syntax parent)
        {
            var right = parent.Right;
            var left = parent.Left;
            Tracer.Assert(right == null);
            Tracer.Assert(left.TokenClass is Case);
            Tracer.Assert(left.Left == null);

            return left.Right.GetResult(context.InBrackets);
        }
    }
}