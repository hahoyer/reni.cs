using System;
using System.Linq;
using Bnf.CodeItems;
using Bnf.Contexts;
using Bnf.DataTypes;
using Bnf.Features;
using hw.DebugFormatter;
using hw.Helper;

namespace Bnf.Forms
{
    abstract class Form : DumpableObject, IForm
    {
        public static readonly IForm Empty = new Empty();
        protected readonly Syntax Parent;
        readonly FunctionCache<Context, Result> ResultCache;

        protected Form(Syntax parent)
        {
            Parent = parent;
            ResultCache = new FunctionCache<Context, Result>(GetResult);
        }

        Result IForm.GetResult(Context context) => ResultCache[context];

        protected abstract Result GetResult(Context context);
    }

    sealed class IntemediateForm : DumpableObject, IForm
    {
        readonly IssueId IssueId;
        readonly Syntax Parent;

        public IntemediateForm(Syntax parent, IssueId issueId = null)
        {
            IssueId = issueId ?? IssueId.SyntaxError;
            Parent = parent;
        }

        Result IForm.GetResult(Context context) => Result.Create(Parent.Token.Characters, IssueId);

        public IForm Left => Parent.Left?.Form;
        public IForm Right => Parent.Right?.Form;
    }

    sealed class Empty : DumpableObject, IForm, IExpression
    {
        Result IForm.GetResult(Context context) => throw new NotImplementedException();
    }

    sealed class Statements : Form, IStatements
    {
        [EnableDump]
        readonly IStatement[] Data;

        public Statements(Syntax parent, IStatement[] data)
            : base(parent) => Data = data;

        IStatement[] IStatements.Data => Data;

        protected override Result GetResult(Context context)
            => Result.Create
            (
                Parent.Token.Characters,
                DataType.Void,
                Data.Select(st => st.GetResult(context).ToVoid.CodeItems).Aggregate()
            );
    }

    sealed class List : Form, IList
    {
        readonly IForm[] Data;

        List(Syntax parent, IForm[] data)
            : base(parent) => Data = data;

        public List(Syntax parent, IForm a, IForm b)
            : this(parent, new[] {a, b}) {}

        public List(Syntax parent, IForm a, IForm[] b)
            : this(parent, new[] {a}.Concat(b).ToArray()) {}

        IForm[] IList.Data => Data;
        protected override Result GetResult(Context context) => throw new NotImplementedException();
    }
}