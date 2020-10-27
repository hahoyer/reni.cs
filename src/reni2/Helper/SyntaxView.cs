using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.SyntaxTree;

namespace Reni.Helper
{
    public abstract class SyntaxView<TResult> : DumpableObject, ValueCache.IContainer, ITree<TResult>
        where TResult : SyntaxView<TResult>
    {
        class CacheContainer
        {
            public FunctionCache<int, TResult> DirectChildren;
            public FunctionCache<int, (TResult, int)> LocateByPosition;
            public FunctionCache<int, (TResult, int)> LocateByPositionIncludingParent;
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

            foreach(var anchor in FlatItem.FrameItems.Items)
                Context[anchor] = (TResult)this;

            Cache.LocateByPosition = new FunctionCache<int, (TResult, int)>(i => LocateByPositionForCache(i, false));
            Cache.LocateByPositionIncludingParent 
                = new FunctionCache<int, (TResult, int)>(i=>LocateByPositionForCache(i, true));
            Tracer.ConditionalBreak(flatItem.ObjectId == -492);
        }

        [DisableDump]
        public SourcePart SourcePart => FlatItem.FrameItems.SourcePart;

        [DisableDump]
        internal TResult LeftMost => this.GetNodesFromLeftToRight().First();

        [DisableDump]
        internal TResult RightMost => this.GetNodesFromRightToLeft().First();

        [DisableDump]
        internal IEnumerable<int> ParserLevelGroup
            => this.CachedValue(GetParserLevelGroup);

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

        (TResult, int) LocateByPositionForCache(int current, bool includingParent)
        {
            NotImplementedMethod(current, includingParent);
            return default;
        }

        internal(TResult Master, int Index) LocateByPosition(int offset, bool includingParent) 
            => includingParent? Cache.LocateByPositionIncludingParent[offset] : Cache.LocateByPosition[offset];

        IEnumerable<int> GetParserLevelGroup()
        {
            NotImplementedMethod();
            return default;
        }

        TResult[] GetRightChildren()
            => (DirectChildCount - LeftDirectChildCount)
                .Select(index => DirectChildren[index + LeftDirectChildCount])
                .ToArray();
    }
}