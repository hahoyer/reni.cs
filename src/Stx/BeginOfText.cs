using hw.DebugFormatter;
using hw.Parser;

namespace Stx
{
    sealed class BeginOfText : TokenClass
    {
        const string TokenId = PrioTable.BeginOfText;

        [DisableDump]
        public override string Id => TokenId;
    }
}