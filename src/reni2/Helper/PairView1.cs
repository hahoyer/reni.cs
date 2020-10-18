using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Helper
{
    [Obsolete("",true)]
    abstract class PairView1<TResult> : DumpableObject, ValueCache.IContainer, ITree<TResult>
        where TResult : PairView1<TResult>
    {
        class CacheContainer
        {
            public ValueCache<BinaryView<TResult>> Binary;
            public FunctionCache<int, TResult> LocateByPosition;
            public ValueCache<SyntaxView1<TResult>> Syntax;
        }

        readonly CacheContainer Cache = new CacheContainer();

        protected PairView1(BinaryTree flatItem, TResult parent, Func<Syntax> getFlatSyntax)
        {
            Cache.LocateByPosition = new FunctionCache<int, TResult>(LocateByPositionForCache);
            Cache.Binary = ValueCacheExtension.NewValueCache(() => GetBinary(flatItem, parent));
            Cache.Syntax = ValueCacheExtension.NewValueCache(() => GetSyntax(getFlatSyntax, parent));
        }

        public ITokenClass TokenClass => Binary.FlatItem.TokenClass;

        internal BinaryView<TResult> Binary => Cache.Binary.Value;
        internal SyntaxView1<TResult> Syntax => Cache.Syntax.Value;

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

        [DisableDump]
        internal IEnumerable<TResult> ParserLevelBelongings => this.CachedValue(GetParserLevelBelongings);

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        protected override string GetNodeDump() => base.GetNodeDump() + $"({Binary.FlatItem?.TokenClass.Id})";

        TResult LocateByPositionForCache(int current)
        {
            var nodes = this
                .GetNodesFromLeftToRight(node => node.Binary)
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

        BinaryView<TResult> GetBinary(BinaryTree flatItem, TResult parent)
        {
            var left = flatItem.Left == null? null : Create(flatItem.Left);
            var right = flatItem.Right == null? null : Create(flatItem.Right);

            return new BinaryView<TResult>(flatItem, left, right, parent, (TResult)this);
        }

        protected abstract TResult Create(BinaryTree flatItem);
        protected abstract TResult Create(Syntax flatItem);

        SyntaxView1<TResult> GetSyntax(Func<Syntax> getFlatSyntax, TResult parent)
        {
            var flatItem = GetFlatSyntax(getFlatSyntax, parent);

            flatItem.AssertIsNotNull();
            var directChildren = flatItem.DirectChildren.Select(FindOrCreate).ToArray();
            return new SyntaxView1<TResult>(flatItem, directChildren, parent, (TResult)this);
        }

        TResult FindOrCreate(Syntax flatItem, int index)
        {
            if(flatItem == null)
                return null;

            if(flatItem.Anchor == null)
                return Create(flatItem);

            var path = Binary.FlatItem.GetPath(node => node == flatItem.Anchor);
            path.AssertIsNotNull();
            return (TResult)this.ApplyPath(path, node => node.Binary);
        }

        Syntax GetFlatSyntax(Func<Syntax> getFlatSyntax, TResult parent)
        {
            var flatItem = getFlatSyntax?.Invoke();
            if(flatItem != null)
                return flatItem;

            var parentSyntax = parent.Syntax.FlatItem;

            var flatItemDirectChildren = parentSyntax.DirectChildren;
            flatItem = flatItemDirectChildren
                .FirstOrDefault(syntax => IsPartner(syntax, parent));

            if(flatItem == null)
            {
                (TokenClass is IRightBracket).Assert();
                flatItem = new ProxySyntax.RightBracket(parentSyntax, Binary.FlatItem);
            }
            else
                (!(TokenClass is IRightBracket)).Assert();

            return flatItem;
        }

        bool IsPartner(Syntax pair, TResult parent)
        {
            NotImplementedMethod(pair, parent);
            
            return pair?.Anchor == Binary.FlatItem;
        }

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

            (TokenClass is List).Assert();

            var parents = Binary.Parent.Chain(node => node.Binary.Parent)
                .Where(node => matcher.IsBelongingTo(node.TokenClass));

            var children = Right
                .Chain(node => node.Right)
                .TakeWhile(node => matcher.IsBelongingTo(node.TokenClass));

            foreach(var node in parents.Concat(children))
                yield return node;
        }

        int ITree<TResult>.LeftDirectChildCount => throw new NotImplementedException();
        int ITree<TResult>.DirectChildCount => throw new NotImplementedException();
        TResult ITree<TResult>.GetDirectChild(int index) => throw new NotImplementedException();
    }
}