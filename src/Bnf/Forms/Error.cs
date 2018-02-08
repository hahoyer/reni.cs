using System;
using Bnf.Contexts;

namespace Bnf.Forms
{
    sealed class Error<T> : IForm, IError
        where T : class
    {
        readonly IForm Form;
        readonly Syntax Parent;

        public Error(Syntax parent, IForm form)
        {
            Parent = parent;
            Form = form;
        }

        string IForm.GetResult(IContext context) => throw new NotImplementedException();

        string Message => "Form is not " + typeof(T).Name;
    }

    sealed class Error : IForm, IError
    {
        readonly IssueId IssueId;
        readonly Syntax Parent;

        public Error(Syntax parent, IssueId issueId)
        {
            Parent = parent;
            IssueId = issueId;
        }

        string IForm.GetResult(IContext context) => throw new NotImplementedException();
    }
}