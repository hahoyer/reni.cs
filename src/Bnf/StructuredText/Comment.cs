using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText
{
    [BelongsTo(typeof(ScannerTokenFactory))]
    sealed class Comment : DumpableObject, IScannerTokenType, IMatchProvider
    {
        static readonly IssueId MissingEndOfComment = IssueId.MissingEndOfComment;
        readonly Match MatchExpression = "(*" + "*)".Box().Find.Else(Match.End.Find + MissingEndOfComment);

        int? IMatchProvider.Function(SourcePosn sourcePosn)
            => sourcePosn.Match(MatchExpression);

        IParserTokenFactory IScannerTokenType.ParserTokenFactory => null;
        string IScannerTokenType.Id => "comment";
    }
}