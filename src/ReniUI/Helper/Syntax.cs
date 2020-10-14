using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Helper;
using Reni.TokenClasses;
using Reni.Validation;

namespace ReniUI.Helper
{
    sealed class Syntax : BinaryTreeSyntaxWithParent<Syntax>
    {
        class CacheContainer
        {
            public FunctionCache<int, Syntax> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        readonly Func<Reni.Parser.Syntax> GetFlatSyntaxRoot;

        internal Syntax(BinaryTree binary, Syntax parent = null, Func<Reni.Parser.Syntax> getFlatSyntax = null)
            : base(binary, parent)
        {
            GetFlatSyntaxRoot = getFlatSyntax;
            Cache.LocateByPosition = new FunctionCache<int, Syntax>(LocateByPositionForCache);
        }

        [DisableDump]
        internal Reni.Parser.Syntax FlatSyntax
            => this.CachedValue(() => GetFlatSyntaxRoot != null? GetFlatSyntaxRoot() : GetFlatSyntax());

        internal new BinaryTree FlatItem => base.FlatItem;

        [DisableDump]
        internal Issue[] Issues
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        internal string[] DeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        internal IEnumerable<Syntax> ParserLevelBelongings => this.CachedValue(GetParserLevelBelongings);

        IEnumerable<Syntax> GetParserLevelBelongings()
        {
            if(!(TokenClass is IBelongingsMatcher matcher))
                yield break;

            switch(TokenClass)
            {
                case ILeftBracket _ when matcher.IsBelongingTo(Parent.TokenClass):
                    yield return Parent;
                    yield break;
                case ILeftBracket _:
                    NotImplementedMethod();
                    yield break;
                case IRightBracket _ when Left != null && matcher.IsBelongingTo(Left.TokenClass):
                    yield return Left;
                    yield break;
                case IRightBracket _ when Parent.TokenClass is ILeftBracket:
                    yield return Parent;
                    yield break;
                case IRightBracket _:
                    NotImplementedMethod();
                    yield break;
                case List _:
                    break;
                default:
                    NotImplementedMethod();
                    yield break;
            }

            Tracer.Assert(TokenClass is List);

            var parents = Parent
                .Chain(node => node.Parent)
                .Where(node => matcher.IsBelongingTo(node.TokenClass));

            var children = Right
                .Chain(node => node.Right)
                .TakeWhile(node => matcher.IsBelongingTo(node.TokenClass));

            foreach(var node in parents.Concat(children))
                yield return node;
        }

        Reni.Parser.Syntax GetFlatSyntax()
        {
            NotImplementedMethod();
            return default;
        }

        protected override Syntax Create(BinaryTree flatItem) => new Syntax(flatItem, this);


        internal Syntax LocateByPosition(int offset) => Cache.LocateByPosition[offset];

        internal Syntax Locate(SourcePart span)
        {
            NotImplementedMethod(span);
            return default;
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => this
                .GetNodesFromLeftToRight()
                .SelectMany(node => node?.ItemsAsLongAs(condition) ?? new Syntax[0]);

        Syntax LocateByPositionForCache(int current)
        {
            var nodes = this
                .GetNodesFromLeftToRight()
                .ToArray();
            var ranges = nodes
                .Select(node => node.Token.Characters)
                .ToArray();

            return nodes
                .Top(node => node.Token.Characters.EndPosition > current)
                .AssertNotNull();
        }
    }

    class ProxySyntax : Reni.Parser.Syntax.NoChildren
    {
        [DisableDump]
        internal readonly Syntax Client;

        public ProxySyntax(Syntax client, BinaryTree target)
            : base(target)
            => Client = client;
    }
}