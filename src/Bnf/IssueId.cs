using Bnf.Features;
using Bnf.Forms;
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

    static class Extension
    {
        public static Result At(this IssueId issueId, SourcePart position) => Result.Create(position, issueId);

        public static IForm Checked<T>(this IForm form, Syntax parent)
            where T : class
        {
            if((IForm) (form as T) != null)
                return (IForm) (T) form;


            return new Error<T>(parent, form);
        }

        public static IStatement[] MakeStatements(this IForm form)
        {
            switch(form)
            {
                case null: return new IStatement[0];
                case IStatements statements: return statements.Data;
            }

            return null;
        }
    }
}