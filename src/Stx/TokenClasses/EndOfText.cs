using hw.DebugFormatter;
using hw.Parser;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;

namespace Stx.TokenClasses
{
    sealed class EndOfText : TokenClass, IBracketMatch<Syntax>
    {
        sealed class Matched : DumpableObject, IParserTokenType<Syntax>
        {
            Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
            {
                Tracer.Assert(right == null);
                Tracer.Assert(left.Right == null);
                Tracer.Assert(left.Left.Left == null);
                return left.Left.Right;
            }

            string IParserTokenType<Syntax>.PrioTableId => "<frame>";
        }

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value {get;} = new Matched();
        public override string Id => PrioTable.EndOfText;

        protected override Result GetResult
            (Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}