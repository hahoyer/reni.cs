using Bnf.Contexts;
using Bnf.Features;

namespace Bnf.Forms
{
    interface IForm
    {
        Result GetResult(Context context);
    }

    interface IError {}

    interface IExpression : IForm {}

    interface IConstant {}

    interface IStatements : IForm, IListForm<IStatement> {}

    interface IStatement : IForm {}

    interface IDefine : IForm {}
}