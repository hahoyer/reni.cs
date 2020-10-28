using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;
using Reni.Parser;

namespace Reni.SyntaxTree
{
    public sealed class Anchor : DumpableObject
    {
        internal BinaryTree[] Items { get; private set; }

        internal int LeftItemCount { get; private set; }

        Anchor() { }

        [DisableDump]
        internal BinaryTree LeftMostRightItem
            => Items
                .Skip(LeftItemCount)
                .First(item => item != null);

        [DisableDump]
        internal Anchor WithoutLeftMostRightItem
            => new Anchor
            {
                Items = Items
                    .Where((item, index) => index != LeftItemCount)
                    .ToArray()
                , LeftItemCount = LeftItemCount
            };

        [DisableDump]
        internal Anchor Left => new Anchor {Items = LeftItems, LeftItemCount = LeftItemCount};

        [DisableDump]
        internal Anchor Right => new Anchor {Items = RightItems, LeftItemCount = 0};


        [DisableDump]
        BinaryTree[] LeftItems => Items.Take(LeftItemCount).ToArray();

        [DisableDump]
        BinaryTree[] RightItems => Items.Skip(LeftItemCount).ToArray();

        internal SourcePart[] SourcePart => Items.Select(item=>item.Token.SourcePart()).ToArray();
        internal SourcePart SourcePartA => SourcePart.Combine();

        public IEnumerable<Issue> Issues => Items.SelectMany(node => node.Issues);

        public bool IsEmpty => !Items.Any();

        protected override string GetNodeDump() => base.GetNodeDump() + $"[{Items.Length}]";

        internal static Anchor Create(BinaryTree leftAnchor, BinaryTree rightAnchor)
            => new Anchor
            {
                Items = T(leftAnchor, rightAnchor)
                , LeftItemCount = 1
            };

        internal static Anchor Create(BinaryTree leftAnchor)
            => new Anchor
            {
                Items = T(leftAnchor.AssertNotNull())
                , LeftItemCount = 1
            };

        public static Anchor Create(IEnumerable<BinaryTree> left)
            => new Anchor
            {
                Items = left.ToArray()
                , LeftItemCount = left.Count()
            };

        public Anchor Combine(Anchor other)
        {
            if(other == null || !other.Items.Any())
                return this;

            return new Anchor
            {
                Items = other.LeftItems.Concat(Items).Concat(other.RightItems).ToArray()
                , LeftItemCount = other.LeftItemCount + LeftItemCount
            };
        }
    }
}