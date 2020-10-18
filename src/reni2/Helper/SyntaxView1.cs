using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.SyntaxTree;

namespace Reni.Helper
{
    [Obsolete("",true)]
    sealed class SyntaxView1<TResult> : DumpableObject, ValueCache.IContainer, ITree<TResult>
        where TResult : PairView1<TResult>
    {
        internal readonly Syntax FlatItem;
        internal TResult Master { get; }
        readonly TResult[] DirectChildren;
        readonly TResult Parent;

        public SyntaxView1(Syntax flatItem, TResult[] directChildren, TResult parent, TResult master)
        {
            Master = master;
            FlatItem = flatItem;
            DirectChildren = directChildren;
            Parent = parent;
        }

        [DisableDump]
        TResult[] LeftChildren
            => this.CachedValue(() => LeftDirectChildCount.Select(index => DirectChildren[index]).ToArray());

        [DisableDump]
        TResult[] RightChildren => this.CachedValue(GetRightChildren);

        int DirectChildCount => FlatItem.DirectChildren.Length;

        [DisableDump]
        internal TResult LeftMost => Master.GetNodesFromLeftToRight(node => node.Syntax).First();

        [DisableDump]
        internal TResult RightMost => Master.GetNodesFromRightToLeft(node => node.Syntax).First();

        [DisableDump]
        internal TResult LeftNeighbor => RightMostLeftSibling?.Syntax.RightMost ?? LeftParent;

        [DisableDump]
        internal TResult RightNeighbor => LeftMostRightSibling?.Syntax.LeftMost ?? RightParent;

        int LeftDirectChildCount => FlatItem.LeftDirectChildCount;
        [DisableDump]
        internal TResult RightMostLeftSibling => DirectChildren[LeftDirectChildCount - 1];


        [DisableDump]
        internal TResult LeftMostRightSibling => DirectChildren[LeftDirectChildCount];

        [DisableDump]
        internal bool IsLeftChild => Parent?.Syntax.RightMostLeftSibling.Syntax == this;

        [DisableDump]
        internal bool IsRightChild => Parent?.Syntax.LeftMostRightSibling.Syntax == this;

        [DisableDump]
        TResult LeftParent
            => Parent != null && Parent.Syntax.LeftChildren.Any(node => node.Syntax == this)
                ? Parent.Syntax.LeftParent
                : Parent;

        [DisableDump]
        TResult RightParent
            => Parent != null && Parent.Syntax.RightChildren.Any(node => node.Syntax == this)
                ? Parent.Syntax.RightParent
                : Parent;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        int ITree<TResult>.DirectChildCount => DirectChildCount;

        TResult ITree<TResult>.GetDirectChild(int index) => DirectChildren[index];
        int ITree<TResult>.LeftDirectChildCount => LeftDirectChildCount;

        protected override string GetNodeDump() => base.GetNodeDump() + $"({FlatItem.Anchor?.TokenClass.Id})";

        TResult[] GetRightChildren()
            => (DirectChildCount - LeftDirectChildCount)
                .Select(index => DirectChildren[index + LeftDirectChildCount])
                .ToArray();
    }
}