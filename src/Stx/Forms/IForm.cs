using Stx.Contexts;
using Stx.Features;


namespace Stx.Forms
{
    interface IForm
    {
        Result GetResult(Context context);
    }

    interface IError {}

    interface IExpression : IForm {}

    interface IConstant {}

    interface IStatements : IForm
    {
        IStatement[] Data {get;}
    }

    interface IStatement : IForm {}

    interface IList
    {
        IForm[] Data {get;}
    }

    interface IIndex : IForm
    {
        IExpression Value {get;}
    }
}