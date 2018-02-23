using System.Collections.Generic;
using System.Linq;
using Bnf.Base;
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

        OccurenceDictionary<T> GetTokenOccurences<T>(Definitions<T>.IContext context)
            where T : class, IParseSpan, ISourcePartProxy;
    }

    sealed class OccurenceDictionary<T> : DumpableObject
        where T : class, IParseSpan, ISourcePartProxy
    {
        [EnableDump]
        internal readonly IDictionary<ILiteral, object> Data;

        [EnableDump]
        internal readonly string[] ToDo;

        [EnableDump]
        readonly string  Reference;

        public OccurenceDictionary(string reference)
        {
            ToDo = new[] {reference};
            Reference = reference;
        }

        OccurenceDictionary(string[] toDo, IDictionary<ILiteral, object> data)
        {
            ToDo = toDo;
            Data = data;
        }

        public OccurenceDictionary(Literal destination)
        {
            ToDo = new string[0];
            Data = new Dictionary<ILiteral, object> {[destination] = new LiteralOccurence(destination)};
        }

        public OccurenceDictionary<T> AssignTo(string name)
        {
            if(name == Reference)
                NotImplementedMethod(name);

            if(Data == null)
                return this;

            NotImplementedMethod(name);
            return null;
        }

        KeyValuePair<ILiteral, object>
            KeyValuePair(string name, KeyValuePair<ILiteral, object> pair)
            => new KeyValuePair<ILiteral, object>(pair.Key, AssignTo(name, pair.Value));

        object AssignTo(string name, object value)
        {
            NotImplementedMethod(name, value);
            return null;
        }

        public OccurenceDictionary<T> Repeat()
        {
            NotImplementedMethod();
            return null;
        }
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