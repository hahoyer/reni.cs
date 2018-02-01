using hw.Helper;
using hw.Scanner;
using Stx.Features;

namespace Stx
{
    sealed class IssueId : EnumEx, Match.IError
    {
        public static readonly IssueId MissingEndOfComment = new IssueId();
        public static readonly IssueId MissingEndOfPragma = new IssueId();
        public static readonly IssueId InvalidCharacter = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId ReassignDestinationMissing = new IssueId();
        public static readonly IssueId ReassignValueMissing = new IssueId();
    }

    static class Extension
    {
        public static Result At(this IssueId issueId, SourcePart position) => new Result(position) {IssueId = issueId};
    }
}