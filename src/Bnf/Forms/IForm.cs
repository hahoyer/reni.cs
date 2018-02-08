using Bnf.Contexts;

namespace Bnf.Forms
{
    interface IForm
    {
        string GetResult(IContext context);
    }

    interface IError {}

    interface IExpression : IForm {}

    interface IStatements : IForm, IListForm<IStatement> {}

    interface IStatement : IForm {}
}