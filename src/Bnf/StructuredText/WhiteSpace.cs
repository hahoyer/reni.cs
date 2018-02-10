using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText
{
    [BelongsTo(typeof(ScannerTokenFactory))]
    sealed class WhiteSpace : DumpableObject, ILexerTokenType
    {
        string IUniqueIdProvider.Value => "whitespace";
    }
}