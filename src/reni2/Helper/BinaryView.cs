using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.TokenClasses;

namespace Reni.Helper
{
    [Obsolete("",true)]
    sealed class BinaryView<TResult> : DumpableObject, ValueCache.IContainer, ITree<TResult>
        where TResult : PairView1<TResult>
    {
        internal readonly BinaryTree FlatItem;
        internal readonly TResult Left;
        internal readonly TResult Parent;
        internal readonly TResult Right;
        readonly TResult Master;

        public BinaryView(BinaryTree flatItem, TResult left, TResult right, TResult parent, TResult master)
        {
            FlatItem = flatItem;
            Left = left;
            Right = right;
            Parent = parent;
            Master = master;
        }

        [DisableDump]
        internal TResult LeftMost => Master.GetNodesFromLeftToRight(node => node.Binary).First();

        [DisableDump]
        internal TResult RightMost => Master.GetNodesFromRightToLeft(node => node.Binary).First();

        [DisableDump]
        internal TResult LeftNeighbor => Left?.Binary.RightMost ?? LeftParent;

        [DisableDump]
        internal TResult RightNeighbor => Right?.Binary.LeftMost ?? RightParent;

        [DisableDump]
        internal bool IsLeftChild => Parent?.Binary.Left?.Binary == this;

        [DisableDump]
        internal bool IsRightChild => Parent?.Binary.Right?.Binary == this;

        [DisableDump]
        TResult LeftParent
            => Parent != null && Parent.Binary.Left?.Binary == this
                ? Parent.Binary.LeftParent
                : Parent;

        [DisableDump]
        TResult RightParent
            => Parent != null && Parent.Binary.Right?.Binary == this
                ? Parent.Binary.RightParent
                : Parent;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        [DisableDump]
        int ITree<TResult>.DirectChildCount => 2;

        TResult ITree<TResult>.GetDirectChild(int index)
            => index switch
            {
                0 => Left
                , 1 => Right
                , _ => null
            };

        [DisableDump]
        int ITree<TResult>.LeftDirectChildCount => 1;

        protected override string GetNodeDump() => base.GetNodeDump() + $"({FlatItem.TokenClass.Id})";
    }
}