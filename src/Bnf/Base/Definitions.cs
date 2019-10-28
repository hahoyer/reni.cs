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
    sealed class Definitions<T> : DumpableObject, IContext<T>
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

            ILiteral[] IDeclaration.Literals {get; set;}

            T Parse(IParserCursor source, Forms.IContext<T> context)
                => Expression.Parse(source, context);

            protected override string GetNodeDump() => base.GetNodeDump() + "." + Name;
        }

        internal interface IOccurences
        {
            IOccurences this[ILiteral literal] {get;}
        }

        static IEnumerable<string> ParserLiterals(IDeclaration declaration)
            => declaration.Items.OfType<ILiteralContainer>().Select(i => i.Value.Value);

        static IEnumerable<ILiteralContainer> GetTerminals(IDeclaration declaration)
            => declaration.Items.OfType<ILiteralContainer>();

        [DisableDump]
        internal readonly IDictionary<string, IDeclaration> Data;

        readonly string RootName;
        IOccurences TokenDictionaryCache;
        IDictionary<string, IOccurences> TokenOccurencesCache;

        internal Definitions(IDictionary<string, IExpression> data, string rootName)
        {
            RootName = rootName;
            Data = data
                .ToDictionary
                    (i => i.Key, i => (IDeclaration) new BnfDefinition(i.Key, i.Value));
        }

        OccurenceDictionary<T> IContext<T>.CreateOccurence(Literal literal)
            => new OccurenceDictionary<T>(literal);

        OccurenceDictionary<T> IContext<T>.CreateOccurence(Define.IDestination destination)
            => new OccurenceDictionary<T>(destination.Name);

        OccurenceDictionary<T> IContext<T>.CreateRepeat(OccurenceDictionary<T> children)
            => children.Repeat();

        OccurenceDictionary<T> IContext<T>.CreateSequence(IExpression[] expressions)
        {
            var children = expressions.Select(data => data.GetTokenOccurences(this)).ToArray();
            NotImplementedMethod(expressions.Stringify(";\n"), nameof(children), children);
            return null;
        }

        IOccurences TokenDictionary => TokenDictionaryCache ?? (TokenDictionaryCache = TokenOccurences[RootName]);

        [DisableDump]
        IDeclaration Root => Data[RootName];

        [DisableDump]
        public IMatch ParserLiteralMatch
            => RelevantDefintions()
                .SelectMany(ParserLiterals)
                .OrderByDescending(i => i.Length)
                .Select(i => i.Box())
                .Aggregate((t, n) => t.Else(n));

        IDictionary<string, IOccurences> TokenOccurences
            => TokenOccurencesCache ?? (TokenOccurencesCache = GetTokenOccurences());


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
            var find = TokenDictionary[l];

            return find;
        }

        Dictionary<string, IOccurences> GetTokenOccurences()
        {
            while(ReplendishLiterals()) {}


            var known = new List<string>();
            string[] toDo = {RootName};
            do
            {
                known.AddRange(toDo);

                var ndt = toDo
                    .Select(name => AssignTo(name, Data[name].Expression.GetTokenOccurences(this)))
                    .ToArray();

                toDo = ndt
                    .SelectMany(dicionary => dicionary.ToDo)
                    .Where(name => !known.Contains(name))
                    .ToArray();

                NotImplementedMethod(nameof(toDo), toDo, nameof(ndt), ndt);
            }
            while(toDo.Any());

            return TokenOccurencesCache.ToDictionary(i => i.Key, i => i.Value);
        }

        bool ReplendishLiterals()
        {
            var result = false;
            foreach(var declaration in Data)
                if(ReplendishLiterals(declaration.Value))
                    result = true;

            return result;
        }

        bool ReplendishLiterals(IDeclaration declaration)
        {
            var literals = declaration.Literals;
            var declarationItems = declaration.Items;
            if(literals == null)
            {
                Tracer.ConditionalBreak
                (
                    declaration.Name == "simple_type_name");
                literals = declarationItems
                    .OfType<ILiteralContainer>()
                    .Select(i => i.Value)
                    .ToArray();
                declaration.Literals = literals;
                return true;
            }

            var newLiterals = declarationItems
                .OfType<Define.IDestination>()
                .SelectMany(GetLiterals)
                .Distinct()
                .Where(l => !literals.Contains(l))
                .ToArray();

            var result = newLiterals.Any();
            if(result)
                declaration.Literals = declaration.Literals.Concat(newLiterals).ToArray();
            return result;
        }

        ILiteral[] GetLiterals(Define.IDestination i)
            => Data.TryGetValue(i.Name, out var result) ? result.Literals : new ILiteral[0];

        OccurenceDictionary<T> AssignTo(string name, OccurenceDictionary<T> assignTo)
        {
            NotImplementedMethod(name, assignTo);
            return null;
        }
    }
}