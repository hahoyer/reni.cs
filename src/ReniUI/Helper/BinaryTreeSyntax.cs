using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.TokenClasses;

namespace ReniUI.Helper
{
    sealed class BinaryTreeSyntax : BinaryTreeSyntaxWithParent<BinaryTreeSyntax>
    {
        class CacheContainer
        {
            public FunctionCache<int, BinaryTreeSyntax> ExtendedLocateByPosition;
            public FunctionCache<int, BinaryTreeSyntax> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        internal BinaryTreeSyntax(BinaryTree target)
            : this(target, null) { }

        BinaryTreeSyntax(BinaryTree target, BinaryTreeSyntax parent)
            : base(target, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, BinaryTreeSyntax>(LocateByPositionForCache);
            Cache.ExtendedLocateByPosition = new FunctionCache<int, BinaryTreeSyntax>(ExtendedLocateByPositionForCache);
        }

        [DisableDump]
        internal IEnumerable<BinaryTreeSyntax> ParentChainIncludingThis
        {
            get
            {
                yield return this;

                if(Parent == null)
                    yield break;

                foreach(var other in Parent.ParentChainIncludingThis)
                    yield return other;
            }
        }

        public BinaryTreeSyntax LocateByPosition(int current) => Cache.LocateByPosition[current];

        protected override BinaryTreeSyntax Create(BinaryTree target, BinaryTreeSyntax parent) => new BinaryTreeSyntax(target, parent);

        internal BinaryTreeSyntax Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
               Right?.CheckedLocate(part) ??
               this;

        BinaryTreeSyntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        BinaryTreeSyntax LocatePositionExtended(int current) => Cache.ExtendedLocateByPosition[current];

        BinaryTreeSyntax LocateByPositionForCache(int current)
            => Left?.LocateByPositionOrDefault(current) ??
               Right?.LocateByPositionOrDefault(current) ??
               this;

        BinaryTreeSyntax ExtendedLocateByPositionForCache(int current)
            => Contains(current)
                ? Left?.ExtendedLocateByPositionOrDefault(current) ??
                  Right?.ExtendedLocateByPositionOrDefault(current) ??
                  this
                : Parent.ExtendedLocateByPositionForCache(current);

        BinaryTreeSyntax LocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocateByPosition(current)
                    : null;

        BinaryTreeSyntax ExtendedLocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocatePositionExtended(current)
                    : null;
    }
}