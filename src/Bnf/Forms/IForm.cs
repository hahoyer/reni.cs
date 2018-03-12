using System.Collections;
using System.Collections.Generic;
using Bnf.Parser;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace Bnf.Forms
{
    interface IForm {}

    interface IError {}

    interface IExpression : IForm
    {
        IEnumerable<IExpression> Children {get;}
        int? Match(SourcePosn sourcePosn, IScannerContext scannerContext);

        T Parse<T>(IParserCursor cursor, IContext<T> context)
            where T : class, ISourcePartProxy, IParseSpan;

        OccurenceDictionary<T> GetTokenOccurences<T>(Base.IContext<T> context)
            where T : class, IParseSpan, ISourcePartProxy;
    }

    sealed class LiteralOccurence : DumpableObject
    {
        readonly Literal Literal;

        public LiteralOccurence(Literal literal) => Literal = literal;
    }

    interface ILiteral : IUniqueIdProvider {}

    interface IParserCursor
    {
        int Position {get;}
        IParserCursor Add(int value);
        IParserCursor TryDeclaration(string name);
    }

    interface IParseSpan
    {
        int Value {get;}
    }

    interface IDeclarationContext
    {
        IDeclaration this[string name] {get;}
    }

    interface IContext<T> : IDeclarationContext
        where T : class, IParseSpan, ISourcePartProxy
    {
        TokenGroup this[IParserCursor source] {get;}
        T Repeat(IEnumerable<T> parseData);
        T Sequence(IEnumerable<T> data);
        T LiteralMatch(TokenGroup token);
    }

    interface IDeclaration
    {
        string Name {get;}
        IExpression Expression {get;}
        IEnumerable<IExpression> Items {get;}
        ILiteral[] Literals {get; set;}
    }

    interface IHiearachicalItem<T>
        where T : class, IParseSpan, ISourcePartProxy
    {
        string Name {get;}
        T Parse(IParserCursor source, IContext<T> context);
    }

    interface IStatements : IForm, IListForm<IStatement> {}

    interface IStatement : IForm
    {
        string Name {get;}
        IExpression Value {get;}
    }
}