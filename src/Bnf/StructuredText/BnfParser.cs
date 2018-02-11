using System.Collections.Generic;
using System.Linq;
using Bnf.Base;
using Bnf.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText
{
    sealed class BnfParser<T> : DumpableObject, IParser<T>
        where T : class, ISourcePartProxy, IParseSpan
    {
        sealed class Cursor : DumpableObject, IParserCursor
        {
            int Position;

            public Cursor() {}

            Cursor(int position) => Position = position;

            IParserCursor IParserCursor.Clone => new Cursor(Position);
            int IParserCursor.Current => Position;
            void IParserCursor.Add(int value) {Position += value;}
        }

        sealed class Context : DumpableObject, IContext<T>
        {
            readonly List<TokenGroup> Items = new List<TokenGroup>();

            readonly BnfParser<T> Parent;
            readonly SourcePosn SourcePosition;

            public Context(BnfParser<T> parent, SourcePosn sourcePosition)
            {
                Parent = parent;
                SourcePosition = sourcePosition;
            }

            IDeclaration<T> IContext<T>.this[string name] => Parent.Definitions.Data[name];

            T IContext<T>.Repeat(IEnumerable<T> data)
            {
                if(!data.Any())
                    return Parent.ResultFactory.EmptyRepeat;

                NotImplementedMethod(data.ToArray().Stringify(";"));
                return null;
            }

            T IContext<T>.Sequence(IEnumerable<T> data)
            {
                if(!data.Any())
                    return Parent.ResultFactory.EmptySequence;

                NotImplementedMethod(data.ToArray().Stringify(";"));
                return null;
            }

            TokenGroup IContext<T>.this[IParserCursor source]
            {
                get
                {
                    var position = source.Current;

                    while(position >= Items.Count)
                    {
                        var item = Parent.Scanner.GetNextTokenGroup(SourcePosition);
                        if(item.Type is EndOfText)
                            return item;
                        Items.Add(item);
                    }

                    return Items[position];
                }
            }
        }

        readonly Definitions<T> Definitions;
        readonly IScanner Scanner;
        readonly string Start;
        readonly IResultFactory<T> ResultFactory;


        public BnfParser(IScanner scanner, Definitions<T> definitions, string start, IResultFactory<T> resultFactory)
        {
            Scanner = scanner;
            Definitions = definitions;
            Start = start;
            ResultFactory = resultFactory;
        }

        public bool Trace {get; set;}

        T IParser<T>.Execute(SourcePosn position)
            => Definitions.Data[Start].Parse(new Cursor(), new Context(this, position));
    }

    interface IResultFactory<out T>
    {
        T EmptyRepeat {get;}
        T EmptySequence {get;}
    }
}