using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;

namespace ReniUI.Helper
{
    sealed class BinaryTree : BinaryTreeSyntaxWithParent<BinaryTree>
    {
        class CacheContainer
        {
            public FunctionCache<int, BinaryTree> ExtendedLocateByPosition;
            public FunctionCache<int, BinaryTree> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        internal BinaryTree(Reni.TokenClasses.BinaryTree flatItem)
            : this(flatItem, null) { }

        BinaryTree(Reni.TokenClasses.BinaryTree flatItem, BinaryTree parent)
            : base(flatItem, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, BinaryTree>(LocateByPositionForCache);
            Cache.ExtendedLocateByPosition = new FunctionCache<int, BinaryTree>(ExtendedLocateByPositionForCache);
        }

        [DisableDump]
        internal IEnumerable<BinaryTree> ParentChainIncludingThis
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

        public BinaryTree LocateByPosition(int current) => Cache.LocateByPosition[current];

        protected override BinaryTree Create
            (Reni.TokenClasses.BinaryTree target, BinaryTree parent) => new BinaryTree(target, parent);

        internal BinaryTree Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
               Right?.CheckedLocate(part) ??
               this;

        BinaryTree CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        BinaryTree LocatePositionExtended(int current) => Cache.ExtendedLocateByPosition[current];

        BinaryTree LocateByPositionForCache(int current)
            => Left?.LocateByPositionOrDefault(current) ??
               Right?.LocateByPositionOrDefault(current) ??
               this;

        BinaryTree ExtendedLocateByPositionForCache(int current)
            => Contains(current)
                ? Left?.ExtendedLocateByPositionOrDefault(current) ??
                  Right?.ExtendedLocateByPositionOrDefault(current) ??
                  this
                : Parent.ExtendedLocateByPositionForCache(current);

        BinaryTree LocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocateByPosition(current)
                    : null;

        BinaryTree ExtendedLocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocatePositionExtended(current)
                    : null;
    }
}