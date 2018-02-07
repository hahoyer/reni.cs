using Bnf.Forms;

namespace Bnf.TokenClasses
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

    sealed class StringLiteral : Literal
    {
        readonly string Delimiter;

        public StringLiteral(string delimiter) => Delimiter = delimiter;
    }
}