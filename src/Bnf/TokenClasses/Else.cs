using Bnf.Forms;
using Bnf.Scanner;
using hw.DebugFormatter;
using hw.Parser;

namespace Bnf.TokenClasses
{
    [BelongsTo(typeof(TokenFactory))]
    sealed class Else : TokenClass
    {
        public const string TokenId = "else";

        [DisableDump]
        public override string Id => TokenId;

        protected override IForm GetForm(Syntax parent) => new IntemediateForm(parent);
    }
}