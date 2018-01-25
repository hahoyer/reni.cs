namespace Stx
{
    abstract class Literal : TokenClass
    {
        public override string Id => "<Literal>";
    }

    sealed class NumericLiteral : Literal {}
    sealed class StringLiteral1: Literal {}
    sealed class Timeiteral: Literal {}
}