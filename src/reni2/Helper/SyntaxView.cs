using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Helper
{
    abstract class SyntaxView<TResult> : DumpableObject, ValueCache.IContainer, ITree<TResult>
        where TResult : SyntaxView<TResult>
    {
        class CacheContainer
        {
            public FunctionCache<int, TResult> DirectChildren;
            public FunctionCache<int, TResult> LocateByPosition;
        }

        internal readonly PositionDictionary<TResult> Context;

        internal readonly Syntax FlatItem;
        internal readonly TResult Parent;
        readonly CacheContainer Cache = new CacheContainer();
        readonly int Index;

        protected SyntaxView(Syntax flatItem, TResult parent, PositionDictionary<TResult> context, int index)
        {
            flatItem.AssertIsNotNull();
            FlatItem = flatItem;
            Parent = parent;
            Context = context;
            Index = index;
            Context[FlatItem.Anchor] = (TResult)this;
            Cache.LocateByPosition = new FunctionCache<int, TResult>(LocateByPositionForCache);
            Tracer.ConditionalBreak(flatItem.ObjectId == -492);
        }

        internal ITokenClass TokenClass => FlatItem.Anchor.TokenClass;

        [DisableDump]
        internal IToken Token => FlatItem.Anchor.Token;

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

        internal TResult Left => Context[FlatItem.Anchor.Left];
        internal TResult Right => Context[FlatItem.Anchor.Right];

        [DisableDump]
        internal TResult LeftMost => this.GetNodesFromLeftToRight().First();

        [DisableDump]
        internal TResult RightMost => this.GetNodesFromRightToLeft().First();

        [DisableDump]
        internal IEnumerable<TResult> ParserLevelBelongingers
            => this.CachedValue(GetParserLevelBelongings);

        int LeftDirectChildCount => FlatItem.LeftDirectChildCount;
        int DirectChildCount => FlatItem.DirectChildren.Length;

        internal TResult[] DirectChildren
            => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

        [DisableDump]
        internal TResult LeftNeighbor => RightMostLeftSibling?.RightMost ?? LeftParent;

        [DisableDump]
        internal TResult RightNeighbor => LeftMostRightSibling?.LeftMost ?? RightParent;

        [DisableDump]
        internal TResult RightMostLeftSibling => DirectChildren[LeftDirectChildCount - 1];


        [DisableDump]
        internal TResult LeftMostRightSibling => DirectChildren[LeftDirectChildCount];

        [DisableDump]
        TResult LeftParent
            => Parent != null && Parent.LeftChildren.Any(node => node == this)
                ? Parent.LeftParent
                : Parent;

        [DisableDump]
        TResult RightParent
            => Parent != null && Parent.RightChildren.Any(node => node == this)
                ? Parent.RightParent
                : Parent;

        [DisableDump]
        TResult[] LeftChildren
            => this.CachedValue(() => LeftDirectChildCount.Select(index => DirectChildren[index]).ToArray());

        [DisableDump]
        TResult[] RightChildren => this.CachedValue(GetRightChildren);

        [DisableDump]
        internal bool IsLeftChild => Parent?.RightMostLeftSibling == this;

        [DisableDump]
        internal bool IsRightChild => Parent?.LeftMostRightSibling == this;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        int ITree<TResult>.DirectChildCount => DirectChildCount;
        TResult ITree<TResult>.GetDirectChild(int index) => DirectChildren[index];

        int ITree<TResult>.LeftDirectChildCount => LeftDirectChildCount;

        TResult GetDirectChild(int index)
        {
            var child = FlatItem.DirectChildren[index];
            return child == null? null : Create(child, index);
        }

        protected abstract TResult Create(Syntax syntax, int index);

        protected override string GetNodeDump() => base.GetNodeDump() + $"({TokenClass.Id})";

        TResult LocateByPositionForCache(int current)
        {
            var nodes = this
                .GetNodesFromLeftToRight()
                .ToArray();
            var ranges = nodes
                .Select(node => node.Token.Characters)
                .ToArray();

            if(current < nodes.Top(enableEmpty:false).Token.SourcePart().Position)
                return null;

            return nodes
                .Top(node => node.Token.Characters.EndPosition > current);
        }

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

        internal TResult LocateByPosition(int offset) => Cache.LocateByPosition[offset];

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
                case ILeftBracket _ when Parent.RightMost.TokenClass is IRightBracket:
                    yield return Parent.RightMost ;
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

            var parents = Parent.Chain(node => node.Parent)
                .Where(node => matcher.IsBelongingTo(node.TokenClass));

            var children = Right
                .Chain(node => node.Right)
                .TakeWhile(node => matcher.IsBelongingTo(node.TokenClass));

            foreach(var node in parents.Concat(children))
                yield return node;
        }

        TResult[] GetRightChildren()
            => (DirectChildCount - LeftDirectChildCount)
                .Select(index => DirectChildren[index + LeftDirectChildCount])
                .ToArray();
    }
}