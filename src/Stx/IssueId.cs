using hw.Helper;
using hw.Scanner;
using Stx.Features;
using Stx.Forms;

namespace Stx
{
    sealed class IssueId : EnumEx, Match.IError
    {
        public static readonly IssueId MissingEndOfComment = new IssueId();
        public static readonly IssueId MissingEndOfPragma = new IssueId();
        public static readonly IssueId InvalidCharacter = new IssueId();
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId SyntaxError = new IssueId();
    }

    static class Extension
    {
        public static Result At(this IssueId issueId, SourcePart position) => new Result(position) {IssueId = issueId};

        public static IForm Checked<T>(this IForm form, Syntax parent)
            where T : class
        {
            if((IForm) (form as T) != null)
                return (IForm) (T) form;


                return new Error<T>(parent, form);
        }
    }

}