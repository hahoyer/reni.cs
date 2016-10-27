using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni
{
    public class CompilerBase : DumpableObject
    {
        sealed class ComponentData : DumpableObject
        {
            readonly IDictionary<System.Type, object> Components =
                new Dictionary<System.Type, object>();

            readonly PrioTable PrioTable;
            readonly ITokenFactory TokenFactory;

            readonly ValueCache<IParser<Syntax>> ParserCache;
            readonly ValueCache<ISubParser<Syntax>> SubParserCache;
            readonly Func<Syntax, IParserTokenType<Syntax>> Converter;

            internal IParser<Syntax> Parser => ParserCache.Value;
            internal ISubParser<Syntax> SubParser => SubParserCache.Value;

            internal ComponentData
            (
                PrioTable prioTable,
                ITokenFactory tokenFactory,
                Func<Syntax, IParserTokenType<Syntax>> converter)
            {
                PrioTable = prioTable;
                TokenFactory = tokenFactory;
                Converter = converter;
                ParserCache = new ValueCache<IParser<Syntax>>(CreateParser);
                SubParserCache = new ValueCache<ISubParser<Syntax>>(CreateSubParser);
            }

            ISubParser<Syntax> CreateSubParser() => new SubParser<Syntax>(Parser, Converter);

            IParser<Syntax> CreateParser()
            {
                if(PrioTable == null)
                    return null;

                return new PrioParser<Syntax>
                (
                    PrioTable,
                    new TwoLayerScanner(new CachingTokenFactory(TokenFactory)),
                    new BeginOfText()
                );
            }

            internal ComponentData ReCreate
                (
                    PrioTable prioTable,
                    ITokenFactory tokenFactory,
                    Func<Syntax, IParserTokenType<Syntax>> converter
                )
                =>
                new ComponentData
                    (prioTable ?? PrioTable, tokenFactory ?? TokenFactory, converter ?? Converter);

            internal T Get<T>() => (T) Components[typeof(T)];
            internal void Add<T>(T value) => Components.Add(typeof(T), value);
        }

        public interface IComponent
        {
            Component Current { set; }
        }

        readonly IDictionary<object, ComponentData> Dictionary =
            new Dictionary<object, ComponentData>();
        readonly object EmptyTag = new object();

        internal Component this[object tag] => new Component(this, tag);

        public sealed class Component
        {
            public readonly CompilerBase Parent;
            public readonly object Tag;

            internal Component(CompilerBase parent, object tag)
            {
                Parent = parent;
                Tag = tag;
            }

            public PrioTable PrioTable { set { Parent.Define(value, null, null, Tag); } }

            public ITokenFactory TokenFactory
            {
                set
                {
                    Parent.Define(null, value, null, Tag);
                    var component = value as IComponent;
                    if(component != null)
                        component.Current = this;
                }
            }

            public Func<Syntax, IParserTokenType<Syntax>> BoxFunction
            {
                set { Parent.Define(null, null, value, Tag); }
            }

            public IParser<Syntax> Parser => Parent.Dictionary[Tag].Parser;
            public ISubParser<Syntax> SubParser => Parent.Dictionary[Tag].SubParser;
            public T Get<T>() => Parent.Dictionary[Tag].Get<T>();

            public void Add<T>(T value)
            {
                Parent.Dictionary[Tag].Add(value);
                var component = value as IComponent;
                if(component != null)
                    component.Current = this;
            }
        }

        void Define
        (
            PrioTable prioTable,
            ITokenFactory tokenFactory,
            Func<Syntax, IParserTokenType<Syntax>> converter,
            object tag
        )
        {
            ComponentData componentData;
            Dictionary[tag] =
                Dictionary.TryGetValue(tag, out componentData)
                    ? componentData.ReCreate(prioTable, tokenFactory, converter)
                    : new ComponentData(prioTable, tokenFactory, converter);
        }

        public PrioTable PrioTable { set { Define(value, null, null, EmptyTag); } }
        public ITokenFactory TokenFactory { set { Define(null, value, null, EmptyTag); } }
        public IParser<Syntax> Parser => Dictionary[EmptyTag].Parser;
        public T Get<T>() => Dictionary[EmptyTag].Get<T>();
        public void Add<T>(T value) => Dictionary[EmptyTag].Add(value);
    }
}