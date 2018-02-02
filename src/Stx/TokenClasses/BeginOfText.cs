using hw.DebugFormatter;
using hw.Parser;
using Stx.Forms;

namespace Stx.TokenClasses
{
    sealed class BeginOfText : TokenClass
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