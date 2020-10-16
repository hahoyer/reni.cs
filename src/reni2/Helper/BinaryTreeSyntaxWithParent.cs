using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;
using static hw.Helper.ValueCacheExtension;

namespace Reni.Helper
{
    abstract class BinaryTreeSyntaxWithParent<TResult> : TreeWithParentExtended<TResult, BinaryTree>
        where TResult : BinaryTreeSyntaxWithParent<TResult>
    {
        class CacheContainer
        {
            public ValueCache<Syntax> FlatSyntax;
            public FunctionCache<int, TResult> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        protected BinaryTreeSyntaxWithParent(BinaryTree flatItem, TResult parent, Func<Syntax> getFlatSyntax)
            : base(flatItem, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, TResult>(LocateByPositionForCache);
            Cache.FlatSyntax = NewValueCache(() => getFlatSyntax != null? getFlatSyntax() : GetFlatSyntax());
        }

        [DisableDump]
        internal IEnumerable<TResult> ParserLevelBelongings => this.CachedValue(GetParserLevelBelongings);

        IEnumerable<TResult> GetParserLevelBelongings()
        {
            if(!(TokenClass is IBelongingsMatcher matcher))
                yield break;

            switch(TokenClass)
            {
                case ILeftBracket _ when Parent.TokenClass is IRightBracket:
                case IRightBracket _ when Parent.TokenClass is ILeftBracket:
                    yield return Parent;
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

            var parents = Parent
                .Chain(node => node.Parent)
                .Where(node => matcher.IsBelongingTo(node.TokenClass));

            var children = Right
                .Chain(node => node.Right)
                .TakeWhile(node => matcher.IsBelongingTo(node.TokenClass));

            foreach(var node in parents.Concat(children))
                yield return node;
        }

        internal TResult GetRightNeighbor(int current)
            => RightNeighbor
                .Chain(node => node.RightNeighbor)
                .Top(node => node.Token.Characters.EndPosition > current)
                .AssertNotNull();

        internal string FlatSyntaxDump => Cache.FlatSyntax.IsValid? FlatSyntax.Dump() : "<unknown>";

        [DisableDump]
        internal IToken Token => FlatItem.Token;

        [EnableDumpExcept(null)]
        internal IEnumerable<IItem> WhiteSpaces => Token.PrecededWith;

        [DisableDump]
        internal SourcePart SourcePart
        {
            get
            {
                var l = LeftMost.Token.SourcePart();
                var r = RightMost.Token.SourcePart();
                return l.Start.Span(r.End);
            }
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        public TResult Left => ((ITree<TResult>)this).GetDirectChild(0);

        [EnableDump]
        [EnableDumpExcept(null)]
        public TResult Right => ((ITree<TResult>)this).GetDirectChild(1);

        [EnableDump]
        internal ITokenClass TokenClass => FlatItem.TokenClass;

        [DisableDump]
        internal Syntax FlatSyntax => Cache.FlatSyntax.Value;

        internal TResult LocateByPosition(int offset) => Cache.LocateByPosition[offset];

        internal TResult Locate(SourcePart span)
        {
            var locateByPosition = LocateByPosition(span.Position);

            var sourcePositions = locateByPosition
                .Chain(node => node.Parent)
                .Select(node => node.Token.Characters)
                .ToArray();

            return locateByPosition
                .Chain(node => node.Parent)
                .FirstOrDefault(node => span.Contains(node.Token.Characters));
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => this
                .GetNodesFromLeftToRight()
                .SelectMany(node => node?.ItemsAsLongAs(condition) ?? new Syntax[0]);

        TResult LocateByPositionForCache(int current)
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

        internal bool Contains(int current)
            => SourcePart.Position <= current && current < SourcePart.EndPosition;


        Syntax GetFlatSyntax()
        {
            var result = Parent
                .FlatSyntax
                .GetNodesFromTopToBottom(node => node?.Binary != null)
                .SingleOrDefault(node => node.Binary == FlatItem);

            if(result != null)
                return result;

            if(TokenClass is ILeftBracket && TokenClass.IsBelongingTo(Parent.TokenClass))
                return Parent.FlatSyntax;

            if(TokenClass is List)
            {
                if(Parent.TokenClass is ILeftBracket)
                    return Parent.FlatSyntax;

                NotImplementedMethod();
                return default;
            }

            if(TokenClass is Colon)
            {
                if(Parent.TokenClass is ILeftBracket)
                    return Parent.FlatSyntax;
                NotImplementedMethod();
                return default;
            }

            NotImplementedMethod();
            return default;
        }
    }
}