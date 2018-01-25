namespace Stx
{
    abstract class Literal : TokenClass
    {
        public override string Id => "<Literal>";
    }

    sealed class IntegerLiteral : Literal {}

    sealed class Base16Literal : Literal {}

    sealed class Base8Literal : Literal {}

    sealed class Base2Literal : Literal {}

    sealed class RealLiteral : Literal {}
    sealed class StringLiteral1: Literal {}
    sealed class DurationLiteral: Literal {}
    sealed class Timeiteral: Literal {}
    sealed class DateLTimeiteral: Literal {}
    sealed class DateLiteral: Literal {}
}