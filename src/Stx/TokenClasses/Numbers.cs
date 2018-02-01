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

    sealed class NumericLiteral : Literal {}

    sealed class StringLiteral1 : Literal {}

    sealed class Timeiteral : Literal {}
}