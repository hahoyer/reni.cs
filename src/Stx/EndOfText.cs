using hw.DebugFormatter;
using hw.Parser;

namespace Stx {
    sealed class EndOfText : TokenClass
    {
        public override string Id => PrioTable.EndOfText;

        protected override Syntax Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            Tracer.Assert(left.Right == null);
            Tracer.Assert(left.Left.Left == null);
            return left;
        }
    }
}