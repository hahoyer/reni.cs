using System.Collections.Generic;
using Bnf.Parser;
using hw.Scanner;

namespace Bnf.Forms
{
    interface IForm {}

    interface IError {}

    interface IExpression : IForm
    {
        int? Match(SourcePosn sourcePosn, IScannerContext scannerContext);

        T Parse<T>(IParserCursor source, IContext<T> context)
            where T : class, ISourcePartProxy, IParseSpan;

        IEnumerable<IExpression> Children {get;}
    }

    interface ILiteral
    {
        string Value {get;}
    }

    interface IParserCursor
    {
        int Current {get;}
        IParserCursor Clone {get;}
        void Add(int value);
    }

    interface IParseSpan
    {
        int Value {get;}
    }

    interface IContext<T>
        where T : class, IParseSpan, ISourcePartProxy
    {
        IDeclaration<T> this[string name] {get;}
        TokenGroup this[IParserCursor source] {get;}
        T Repeat(IEnumerable<T> parseData);
        T Sequence(IEnumerable<T> data);
    }

    interface IDeclaration<T>
        where T : class, IParseSpan, ISourcePartProxy
    {
        string Name {get;}
        T Parse(IParserCursor source, IContext<T> context);
        IEnumerable<IExpression> Items {get;}
    }

    interface IStatements : IForm, IListForm<IStatement> {}

    interface IStatement : IForm
    {
        string Name {get;}
        IExpression Value {get;}
    }
}