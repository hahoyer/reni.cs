using System.Collections.Generic;
using Bnf.Contexts;
using Bnf.StructuredText;
using hw.Scanner;

namespace Bnf.Forms
{
    interface IForm
    {
        string GetResult(IContext context);
    }

    interface IError {}

    interface IExpression : IForm
    {
        int? Match(SourcePosn sourcePosn, ScannerContext scannerContext);
    }

    interface IStatements : IForm, IListForm<IStatement> {}

    interface IStatement : IForm
    {
        string Name {get;}
        IExpression Value {get;}
    }
}