using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Helper
{
    abstract class PairView<TResult> : DumpableObject, ValueCache.IContainer
        where TResult : PairView<TResult>
    {
        class CacheContainer
        {
            public ValueCache<BinaryView<TResult>> Binary;
            public ValueCache<SyntaxView<TResult>> Syntax;
            public FunctionCache<int, TResult> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        protected PairView(BinaryTree flatItem, TResult parent, Func<Syntax> getFlatSyntax)
        {
            Cache.LocateByPosition = new FunctionCache<int, TResult>(LocateByPositionForCache);
            Cache.Binary = ValueCacheExtension.NewValueCache(() => GetBinary(flatItem, parent));
            Cache.Syntax = ValueCacheExtension.NewValueCache(() => GetSyntax(getFlatSyntax, parent));
        }

        TResult LocateByPositionForCache(int current)
        {
            var nodes = this
                .GetNodesFromLeftToRight(node=>node.Binary)
                .ToArray();
            var ranges = nodes
                .Select(node => node.Token.Characters)
                .ToArray();

            return (TResult)nodes
                .Top(node => node.Token.Characters.EndPosition > current)
                .AssertNotNull();
        }

        internal TResult Locate(SourcePart span)
        {
            var locateByPosition = LocateByPosition(span.Position);

            var sourcePositions = locateByPosition
                .Chain(node => node.Binary.Parent)
                .Select(node => node.Token.Characters)
                .ToArray();

            return locateByPosition
                .Chain(node => node.Binary.Parent)
                .FirstOrDefault(node => span.Contains(node.Token.Characters));
        }

        internal TResult LocateByPosition(int offset) => Cache.LocateByPosition[offset];


        public ITokenClass TokenClass => Binary.FlatItem.TokenClass;

        internal BinaryView<TResult> Binary => Cache.Binary.Value;
        internal SyntaxView<TResult> Syntax => Cache.Syntax.Value;

        [DisableDump]
        internal IToken Token => Binary.FlatItem.Token;

        [EnableDumpExcept(null)]
        internal IEnumerable<IItem> WhiteSpaces => Token.PrecededWith;

        [DisableDump]
        internal SourcePart SourcePart
        {
            get
            {
                var l = Binary.LeftMost.Token.SourcePart();
                var r = Binary.RightMost.Token.SourcePart();
                return l.Start.Span(r.End);
            }
        }

        internal IEnumerable<Issue> Issues => Binary.FlatItem.Issues;

        internal TResult Left => Binary.Left;
        internal TResult Right => Binary.Right;

        internal BinaryTree FlatItem => Binary.FlatItem;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        BinaryView<TResult> GetBinary(BinaryTree flatItem, TResult parent)
        {
            var left = flatItem.Left == null? null : Create(flatItem.Left);
            var right = flatItem.Right == null? null : Create(flatItem.Right);

            return new BinaryView<TResult>(flatItem, left, right, parent, (TResult)this);
        }

        protected abstract TResult Create(BinaryTree flatItem);

        SyntaxView<TResult> GetSyntax(Func<Syntax> getFlatSyntax, TResult parent)
        {
            NotImplementedMethod("getFlatSyntax", parent);
            return default;
        }

        [DisableDump]
        internal IEnumerable<TResult> ParserLevelBelongings => this.CachedValue(GetParserLevelBelongings);

        IEnumerable<TResult> GetParserLevelBelongings()
        {
            if(!(TokenClass is IBelongingsMatcher matcher))
                yield break;

            switch(TokenClass)
            {
                case ILeftBracket _ when Binary.Parent.TokenClass is IRightBracket:
                case IRightBracket _ when Binary.Parent.TokenClass is ILeftBracket:
                    yield return Binary.Parent;
                    yield break;
                case IRightBracket _ when Left != null && matcher.IsBelongingTo(Left.TokenClass):
                    yield return Left;
                    yield break;
                case List _:
                    break;
                default:
                    NotImplementedMethod();
                    yield break;
            }

            Tracer.Assert(TokenClass is List);

            var parents = Binary.Parent.Chain(node => node.Binary.Parent)
                .Where(node => matcher.IsBelongingTo(node.TokenClass));

            var children = Right
                .Chain(node => node.Right)
                .TakeWhile(node => matcher.IsBelongingTo(node.TokenClass));

            foreach(var node in parents.Concat(children))
                yield return node;
        }

    }
}