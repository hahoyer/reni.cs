using hw.Helper;
using hw.Scanner;

namespace Stx
{
    sealed class IssueId : EnumEx, Match.IError
    {
        public static readonly IssueId MissingEndOfComment = new IssueId();
        public static readonly IssueId MissingEndOfPragma = new IssueId();
        public static readonly IssueId InvalidCharacter = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
    }
}