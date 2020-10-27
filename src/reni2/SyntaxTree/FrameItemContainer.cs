using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    public sealed class FrameItemContainer : DumpableObject
    {
        internal BinaryTree[] Items { get; private set; }

        internal int LeftItemCount { get; private set; }

        FrameItemContainer() { }

        [DisableDump]
        internal BinaryTree LeftMostRightItem
            => Items
                .Skip(LeftItemCount)
                .First(item => item != null);

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
        BinaryTree[] LeftItems => Items.Take(LeftItemCount).ToArray();

        [DisableDump]
        BinaryTree[] RightItems => Items.Skip(LeftItemCount).ToArray();

        internal IEnumerable<Issue> Issues
        {
            get { throw new System.NotImplementedException(); }
        }

        internal SourcePart SourcePart
        {
            get { throw new System.NotImplementedException(); }
        }


        protected override string GetNodeDump() => base.GetNodeDump() + $"[{Items.Length}]";

        internal static FrameItemContainer Create(BinaryTree leftAnchor, BinaryTree rightAnchor)
            => new FrameItemContainer
            {
                Items = T(leftAnchor, rightAnchor)
                , LeftItemCount = 1
            };

        internal static FrameItemContainer Create(BinaryTree leftAnchor)
            => new FrameItemContainer
            {
                Items = T(leftAnchor.AssertNotNull())
                , LeftItemCount = 1
            };

        internal static FrameItemContainer Create()
            => new FrameItemContainer
            {
                Items = new BinaryTree[0]
                , LeftItemCount = 0
            };

        public static FrameItemContainer Create(IEnumerable<BinaryTree> left)
            => new FrameItemContainer
            {
                Items = left.ToArray()
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