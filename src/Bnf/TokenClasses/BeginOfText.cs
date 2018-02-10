using Bnf.Forms;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.TokenClasses
{
    sealed class BeginOfText : TokenType
    {
        const string TokenId = PrioTable.BeginOfText;

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent)
        {
            NotImplementedMethod(parent);
            return null;
        }
    }
}