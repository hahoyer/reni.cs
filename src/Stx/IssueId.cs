using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Stx.Features;
using Stx.Forms;
using Stx.TokenClasses;

namespace Stx
{
    sealed class IssueId : EnumEx, Match.IError
    {
        public static readonly IssueId EOLInString = new IssueId();
        public static readonly IssueId InvalidCharacter = new IssueId();
        public static readonly IssueId MissingCaseLabel = new IssueId();
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

        public static IEnumerable<Forms.Case.Clause> SplitForCase(this IForm[] data)
        {
            IConstant label = null;
            List<IStatement> statements = null;

            foreach(var form in data)
                switch(form)
                {
                    case IStatement statement:
                        Tracer.Assert(statements != null);
                        statements.Add(statement);
                        break;
                    case IConstant constant:
                        if(label != null)
                            yield return new Forms.Case.Clause(label, statements);

                        label = constant;
                        statements = new List<IStatement>();
                        break;
                }

            if(label != null)
                yield return new Forms.Case.Clause(label, statements);
        }

        public static IEnumerable<Forms.Case.Clause> CreateLabeledClauses(this Syntax parent)
        {
            var result = new List<Forms.Case.Clause>();

            IConstant label = null;
            List<IStatement> statements = null;

            var current = parent;
            while(current != null)
            {
                var form = current.Left.Form;
                switch(current.TokenClass)
                {
                    case Colon _:
                        if(label != null)
                            result.Add(new Forms.Case.Clause(label, statements));

                        label = form as IConstant;
                        if(label == null)
                            return null;
                        statements = new List<IStatement>();
                        break;
                    case Semicolon _:
                        Tracer.Assert(statements != null);
                        if(!(form is IStatement statement))
                            return null;
                        statements.Add(statement);
                        break;
                }


                current = current.Right;
            }

            if(label != null)
                result.Add(new Forms.Case.Clause(label, statements));

            return result;
        }
    }
}