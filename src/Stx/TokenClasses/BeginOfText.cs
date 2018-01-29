using hw.DebugFormatter;
using hw.Parser;
using Stx.Contexts;
using Stx.DataTypes;
using Stx.Features;

namespace Stx.TokenClasses
{
    sealed class BeginOfText : TokenClass
    {
        const string TokenId = PrioTable.BeginOfText;

        [DisableDump]
        public override string Id => TokenId;

        protected override Result GetResult
            (Context context, Syntax left, IToken token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}