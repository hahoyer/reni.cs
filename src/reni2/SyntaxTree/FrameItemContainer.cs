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

            internal static Dummy Create(BinaryTree anchor) => anchor == null? null : new Dummy(anchor);
        }


        internal Dummy[] Items { get; private set; }

        internal int LeftItemCount { get; private set; }

        internal BinaryTree LeftMostRightItem
            => Items
                .Skip(LeftItemCount)
                .First(item => item != null)
                .Anchor;

        public FrameItemContainer WithoutLeftMostRightItem
            => new FrameItemContainer
            {
                Items = Items
                    .Where((item, index) => index != LeftItemCount)
                    .ToArray()
                , LeftItemCount = LeftItemCount
            };

        internal static FrameItemContainer Create(BinaryTree leftAnchor, BinaryTree rightAnchor)
            => new FrameItemContainer
            {
                Items = T(Dummy.Create(leftAnchor), Dummy.Create(rightAnchor))
                , LeftItemCount = 1
            };
    }
}