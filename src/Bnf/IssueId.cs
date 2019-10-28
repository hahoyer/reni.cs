using hw.Helper;
using hw.Scanner;

namespace Bnf
{
    sealed class IssueId : EnumEx, Match.IError
    {
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId InvalidCharacter = new IssueId();
        public static readonly IssueId MissingEndOfComment = new IssueId();
        public static readonly IssueId MissingEndOfPragma = new IssueId();
        public static readonly IssueId SyntaxError = new IssueId();
    }
}