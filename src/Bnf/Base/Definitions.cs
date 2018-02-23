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
    sealed class Definitions<T> : DumpableObject, Definitions<T>.IContext
        where T : class, IParseSpan, ISourcePartProxy
    {
        sealed class BnfDefinition : DumpableObject, IDeclaration
        {
            [EnableDump]
            readonly IExpression Expression;

            [EnableDump]
            readonly string Name;

            public BnfDefinition(string name, IExpression expression)
            {
                Name = name;
                Expression = expression;
            }

            string IDeclaration.Name => Name;

            IExpression IDeclaration.Expression => Expression;

            IEnumerable<IExpression> IDeclaration.Items
                => Expression.SelectHierachical(parent => parent.Children);

            T Parse(IParserCursor source, IContext<T> context)
                => Expression.Parse(source, context);
        }

        internal interface IOccurences {}

        internal interface IContext
        {
            OccurenceDictionary<T> CreateOccurence(Literal literal);
            OccurenceDictionary<T> CreateOccurence(Define.IDestination destination);
            OccurenceDictionary<T> CreateRepeat(OccurenceDictionary<T> children);
            OccurenceDictionary<T> CreateSequnce(IExpression[] expressions);
        }

        static IEnumerable<string> ParserLiterals(IDeclaration declaration)
            => declaration.Items.OfType<ILiteral>().Select(i => i.Value);

        static IEnumerable<ILiteral> GetTerminals(IDeclaration declaration)
            => declaration.Items.OfType<ILiteral>();

        [DisableDump]
        internal readonly IDictionary<string, IDeclaration> Data;

        readonly string RootName;
        Dictionary<ILiteral, IOccurences> TokenDictionaryCache;

        internal Definitions(IDictionary<string, IExpression> data, string rootName)
        {
            RootName = rootName;
            Data = data
                .ToDictionary
                    (i => i.Key, i => (IDeclaration) new BnfDefinition(i.Key, i.Value));
        }

        OccurenceDictionary<T> IContext.CreateOccurence(Literal literal) 
            => new OccurenceDictionary<T>(literal);

        OccurenceDictionary<T> IContext.CreateOccurence(Define.IDestination destination) 
            => new OccurenceDictionary<T>(destination.Name);

        OccurenceDictionary<T> IContext.CreateRepeat(OccurenceDictionary<T> children) 
            => children.Repeat();

        OccurenceDictionary<T> IContext.CreateSequnce(IExpression[] expressions)
        {
            var children = expressions.Select(data => data.GetTokenOccurences(this)).ToArray();
            NotImplementedMethod(expressions.Stringify(";\n"), nameof(children), children);
            return null;

        }

        [DisableDump]
        IDeclaration Root => Data[RootName];

        [DisableDump]
        public IMatch ParserLiteralMatch
            => RelevantDefintions()
                .SelectMany(ParserLiterals)
                .OrderByDescending(i => i.Length)
                .Select(i => i.Box())
                .Aggregate((t, n) => t.Else(n));

        Dictionary<ILiteral, IOccurences> TokenDictionary
            => TokenDictionaryCache ?? (TokenDictionaryCache = GetTokenDictionary());

        bool IsMatch(ITokenType tokenType, ILiteral literal)
        {
            NotImplementedMethod(tokenType, literal);
            return false;
        }

        IEnumerable<IDeclaration> RelevantDefintions()
            => new[] {RootName}.Closure(Expand).SelectMany(Resolve);

        IEnumerable<IDeclaration> Resolve(string name)
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


        void Register(Type type)
            => Data.AddRange(type.GetBelongings<IDeclaration>().ToDictionary(i => i.Name, i => i));

        public void Register(object target) => Register(target.GetType());

        public IOccurences Find(TokenGroup token)
        {
            var tokenType = token.Type;
            var l = tokenType as ILiteral;
            Tracer.Assert(l != null);
            IOccurences find = TokenDictionary[l];

            return find;
        }

        Dictionary<ILiteral, IOccurences> GetTokenDictionary()
            => GetTokenOccurences().ToDictionary(i => i.Key, i => i.Value);

        IDictionary<ILiteral, IOccurences> GetTokenOccurences()
        {
            var known = new List<string>();
            string[] toDo = {RootName};
            IDictionary<ILiteral, IOccurences> result = new Dictionary<ILiteral, IOccurences>();
            do
            {
                known.AddRange(toDo);

                var ndt = toDo
                    .Select(name => Data[name].Expression.GetTokenOccurences(this).AssignTo(name))
                    .ToArray();

                toDo = ndt
                    .SelectMany(dicionary => dicionary.ToDo)
                    .Where(name => !known.Contains(name))
                    .ToArray();

                NotImplementedMethod(nameof(result), result, nameof(ndt), ndt);
            }
            while(toDo.Any());

            return result;
        }
    }
}