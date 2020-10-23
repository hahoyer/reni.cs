using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class FrameItemContainer : DumpableObject
    {
        internal class Dummy : Syntax.NoChildren
        {
            Dummy(BinaryTree anchor)
                : base(anchor) { }

            protected override string GetNodeDump() => Anchor.NodeDump;
            internal static Dummy Create(BinaryTree anchor) => anchor == null? null : new Dummy(anchor);
        }

        internal Dummy[] Items { get; private set; }

        internal int LeftItemCount { get; private set; }

        FrameItemContainer() { }

        [DisableDump]
        internal BinaryTree LeftMostRightItem
            => Items
                .Skip(LeftItemCount)
                .First(item => item != null)
                .Anchor;

        [DisableDump]
        internal FrameItemContainer WithoutLeftMostRightItem
            => new FrameItemContainer
            {
                Items = Items
                    .Where((item, index) => index != LeftItemCount)
                    .ToArray()
                , LeftItemCount = LeftItemCount
            };

        [DisableDump]
        internal FrameItemContainer Left => new FrameItemContainer {Items = LeftItems, LeftItemCount = LeftItemCount};

        [DisableDump]
        internal FrameItemContainer Right => new FrameItemContainer {Items = RightItems, LeftItemCount = 0};


        [DisableDump]
        Dummy[] LeftItems => Items.Take(LeftItemCount).ToArray();

        [DisableDump]
        Dummy[] RightItems => Items.Skip(LeftItemCount).ToArray();


        protected override string GetNodeDump() => base.GetNodeDump() + $"[{Items.Length}]";

        internal static FrameItemContainer Create(BinaryTree leftAnchor, BinaryTree rightAnchor)
            => new FrameItemContainer
            {
                Items = T(Dummy.Create(leftAnchor), Dummy.Create(rightAnchor))
                , LeftItemCount = 1
            };

        internal static FrameItemContainer Create(BinaryTree leftAnchor)
            => new FrameItemContainer
            {
                Items = T(Dummy.Create(leftAnchor))
                , LeftItemCount = 1
            };

        internal static FrameItemContainer Create()
            => new FrameItemContainer
            {
                Items = new Dummy[0]
                , LeftItemCount = 0
            };

        public static FrameItemContainer Create(IEnumerable<BinaryTree> left)
            => new FrameItemContainer
            {
                Items = left.Select(Dummy.Create).ToArray()
                , LeftItemCount = left.Count()
            };

        public FrameItemContainer Combine(FrameItemContainer other)
        {
            if(other == null || !other.Items.Any())
                return this;

            return new FrameItemContainer
            {
                Items = other.LeftItems.Concat(Items).Concat(other.RightItems).ToArray()
                , LeftItemCount = other.LeftItemCount + LeftItemCount
            };
        }
    }
}