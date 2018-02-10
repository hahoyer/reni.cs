using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText
{
    [BelongsTo(typeof(ScannerTokenFactory))]
    sealed class Identifier : DumpableObject, ITokenType
    {
        string IUniqueIdProvider.Value => "identifier";
    }
}
