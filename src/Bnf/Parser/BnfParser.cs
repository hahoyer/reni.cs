using System.Collections.Generic;
using System.Linq;
using Bnf.Base;
using Bnf.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.Parser
{
    sealed class BnfParser<T> : DumpableObject, IParser<T>
        where T : class, ISourcePartProxy, IParseSpan
    {
        sealed class Cursor : DumpableObject, IParserCursor
        {
            [EnableDumpExcept(null)]
            HashSet<string> Names;

            [EnableDump]
            int Position;

            public Cursor() {}

            Cursor(int position) => Position = position;

            Cursor(int position, IEnumerable<string> names, string name)
            {
                Position = position;
                Names = new HashSet<string> {name};
                if(names == null)
                    return;

                foreach(var items in names)
                    Names.Add(items);
            }

            int IParserCursor.Position => Position;
            IParserCursor IParserCursor.Add(int value) => value > 0 ? new Cursor(Position + value) : this;

            IParserCursor IParserCursor.TryDeclaration(string name)
            {
                if(Names != null && Names.Contains(name))
                    return null;

                return new Cursor(Position, Names, name);

                if(Names == null)
                    Names = new HashSet<string>();
                Names.Add(name);
            }
        }

        sealed class Context : DumpableObject, Forms.IContext<T>
        {
            readonly List<TokenGroup> Items = new List<TokenGroup>();

            readonly BnfParser<T> Parent;
            readonly SourcePosn SourcePosition;

            public Context(BnfParser<T> parent, SourcePosn sourcePosition)
            {
                Parent = parent;
                SourcePosition = sourcePosition;
            }

            IDeclaration IDeclarationContext.this[string name] => Parent.Definitions.Data[name];

            T Forms.IContext<T>.Repeat(IEnumerable<T> data)
            {
                if(!data.Any())
                    return Parent.ResultFactory.EmptyRepeat;

                NotImplementedMethod(data.ToArray().Stringify(";"));
                return null;
            }

            T Forms.IContext<T>.Sequence(IEnumerable<T> data)
                => Parent.ResultFactory.Sequence(data);

            T Forms.IContext<T>.LiteralMatch(TokenGroup token)
                => Parent.ResultFactory.LiteralMatch(token);

            TokenGroup Forms.IContext<T>.this[IParserCursor source]
            {
                get
                {
                    var position = source.Position;

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
        readonly IResultFactory<T> ResultFactory;
        readonly IScanner Scanner;

        public BnfParser(IScanner scanner, Definitions<T> definitions, IResultFactory<T> resultFactory)
        {
            Scanner = scanner;
            Definitions = definitions;
            ResultFactory = resultFactory;
        }

        public bool Trace {get; set;}

        T IParser<T>.Execute(SourcePosn position)
        {

            IParserCursor cursor = new Cursor();
            Forms.IContext<T> context = new Context(this, position);

            while(!(context[cursor].Type is EndOfText))
            {
                var token = context[cursor];
                var matches = Definitions.Find(token);


                cursor = cursor.Add(1);
            }

            NotImplementedMethod(position);
            return null;
        }

        T RootFunction()
        {
            NotImplementedMethod();
            return null;
        }
    }

    interface IResultFactory<T>
    {
        T EmptyRepeat {get;}
        T LiteralMatch(TokenGroup token);
        T Sequence(IEnumerable<T> data);
    }

    sealed class EndOfText : DumpableObject, ITokenType
    {
        string IUniqueIdProvider.Value => "<endoftext>";
    }
}