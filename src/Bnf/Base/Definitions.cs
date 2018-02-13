using System;
using System.Collections.Generic;
using System.Linq;
using Bnf.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.Base
{
    sealed class Definitions<T> : DumpableObject
        where T : class, IParseSpan, ISourcePartProxy
    {
        sealed class BnfDefinition : DumpableObject, IDeclaration<T>
        {
            readonly IExpression Expression;
            readonly string Name;

            public BnfDefinition(string name, IExpression expression)
            {
                Name = name;
                Expression = expression;
            }

            string IDeclaration<T>.Name => Name;

            T IDeclaration<T>.Parse(IParserCursor source, IContext<T> context)
                => Expression.Parse(source, context);

            IEnumerable<IExpression> IDeclaration<T>.Items
                => Expression.SelectHierachical(parent => parent.Children);
        }

        [DisableDump]
        internal readonly IDictionary<string, IDeclaration<T>> Data;

        readonly string RootName;

        internal Definitions(IDictionary<string, IExpression> data, string rootName)
        {
            RootName = rootName;
            Data = data
                .ToDictionary(i => i.Key, i => (IDeclaration<T>) new BnfDefinition(i.Key, i.Value));
        }

        [DisableDump]
        public IDeclaration<T> Root => Data[RootName];

        [DisableDump]
        public IMatch ParserLiteralMatch => RelevantDefintions()
                           .SelectMany(ParserLiterals)
                           .OrderByDescending(i => i.Length)
                           .Select(i => i.Box())
                           .Aggregate((t, n) => t.Else(n));

        static IEnumerable<string> ParserLiterals(IDeclaration<T> declaration) 
            => declaration.Items.OfType<ILiteral>().Select(i => i.Value);

        IEnumerable<IDeclaration<T>> RelevantDefintions() 
            => new[] {RootName}.Closure(Expand).SelectMany(Resolve);

        IEnumerable<IDeclaration<T>> Resolve(string name)
        {
            Data.TryGetValue(name, out var result);
            if(result != null)
                yield return result;
        }

        IEnumerable<string> Expand(string name)
        {
            if(Data.TryGetValue(name, out var value))
                return value
                    .Items
                    .OfType<Define.IDestination>()
                    .Select(i => i.Name);
            return new string[0];
        }

        public void Register(Type type)
            => Data.AddRange(type.GetBelongings<IDeclaration<T>>().ToDictionary(i => i.Name, i => i));

        public void Register(object target) => Register(target.GetType());
    }
}