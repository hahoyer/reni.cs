using Bnf.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;

namespace Bnf.TokenClasses
{
    sealed class EndOfText : TokenType, IBracketMatch<Syntax>
    {
        sealed class Matched : DumpableObject, IPriorityParserTokenType<Syntax>
        {
            Syntax IPriorityParserTokenType<Syntax>.Create(Syntax left, IPrioParserToken token, Syntax right)
            {
                Tracer.Assert(right == null);
                Tracer.Assert(left.Right == null);
                Tracer.Assert(left.Left.Left == null);
                return left.Left.Right;
            }

            string IUniqueIdProvider.Value => "<frame>";
        }

        IPriorityParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new Matched();
        public override string Id => PrioTable.EndOfText;

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }
    }
}