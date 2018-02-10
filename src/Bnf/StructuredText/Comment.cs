using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText
{
    [BelongsTo(typeof(ScannerTokenFactory))]
    sealed class Comment : DumpableObject, ILexerTokenType, INamedMatchProvider
    {
        static readonly IssueId MissingEndOfComment = IssueId.MissingEndOfComment;
        static string TokenId => "comment";
        readonly Match MatchExpression = "(*" + "*)".Box().Find.Else(Match.End.Find + MissingEndOfComment);

        int? IMatchProvider.Function(SourcePosn sourcePosn)
            => sourcePosn.Match(MatchExpression);

        string IUniqueIdProvider.Value => TokenId;
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    sealed class Letter : DumpableObject, INamedMatchProvider
    {
        const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        int? IMatchProvider.Function(SourcePosn sourcePosn) 
            => Letters.Any(c => c == sourcePosn.Current) ? (int?) 1 : null;

        string IUniqueIdProvider.Value => "letter";
    }
}