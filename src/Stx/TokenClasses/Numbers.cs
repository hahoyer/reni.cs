using hw.DebugFormatter;
using Stx.Forms;

namespace Stx.TokenClasses
{
    abstract class Literal : TokenClass
    {
        public override string Id => "<Literal>";

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }
    }

    sealed class RealLiteral : Literal {}

    sealed class IntegerLiteral : Literal
    {
        protected override IForm GetForm(Syntax parent)
        {
            Tracer.Assert(parent.Left == null);
            Tracer.Assert(parent.Right == null);

            return new Integer(parent, long.Parse(parent.Token.Characters.Id));
            return base.GetForm(parent);
        }
    }

    sealed class BinaryLiteral : Literal {}

    sealed class OctalLiteral : Literal {}

    sealed class HexLiteral : Literal {}

    sealed class StringLiteral1 : Literal {}

    sealed class StringLiteral2 : Literal {}

    sealed class DurationLiteral : Literal {}

    sealed class TimeLiteral : Literal {}

    sealed class DateLiteral : Literal {}

    sealed class DateTimeLiteral : Literal {}
}