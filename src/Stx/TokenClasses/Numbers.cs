using hw.Parser;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;

namespace Stx.TokenClasses
{
    abstract class Literal : TokenClass
    {
        public override string Id => "<Literal>";

        protected override Result GetResult(Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }

    sealed class NumericLiteral : Literal {}

    sealed class StringLiteral1 : Literal {}

    sealed class Timeiteral : Literal {}
}