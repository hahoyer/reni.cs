using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText
{
    [BelongsTo(typeof(ScannerTokenFactory))]
    sealed class WhiteSpace : DumpableObject, IScannerTokenType
    {
        IParserTokenFactory IScannerTokenType.ParserTokenFactory => null;
        string IScannerTokenType.Id => "whitespace";
    }
}