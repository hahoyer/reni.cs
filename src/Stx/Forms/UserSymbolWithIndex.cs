using Stx.Contexts;
using Stx.Features;

namespace Stx.Forms
{
    sealed class UserSymbolWithIndex : Form, Reassign.IDestination, IExpression
    {
        public readonly IExpression Index;
        public readonly string Name;

        public UserSymbolWithIndex(Syntax parent, string name, IExpression index)
            : base(parent)
        {
            Name = name;
            Index = index;
        }

        protected override Result GetResult(Context context)
        {
            var index = Index.GetResult(context);

            NotImplementedMethod(context);
            return null;
        }
    }

    sealed class UserSymbolWithExpression : Form, IExpression, IStatement
    {
        public readonly IExpression Expression;
        public readonly string Name;

        public UserSymbolWithExpression(Syntax parent, string name, IExpression expression)
            : base(parent)
        {
            Name = name;
            Expression = expression;
        }

        protected override Result GetResult(Context context)
        {
            NotImplementedMethod(context);
            return null;
        }
    }
}